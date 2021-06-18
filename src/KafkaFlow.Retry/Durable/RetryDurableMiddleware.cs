namespace KafkaFlow.Retry.Durable
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using KafkaFlow;
    using KafkaFlow.Retry.Durable.Definitions;
    using KafkaFlow.Retry.Durable.Repository;
    using KafkaFlow.Retry.Durable.Repository.Actions.Create;
    using Polly;

    internal class RetryDurableMiddleware : IMessageMiddleware
    {
        private readonly ILogHandler logHandler;
        private readonly RetryDurableDefinition retryDurableDefinition;
        private readonly IRetryDurableQueueRepository retryDurableQueueRepository;
        private readonly object syncPauseAndResume = new object();
        private int? controlWorkerId;

        public RetryDurableMiddleware(
            ILogHandler logHandler,
            IRetryDurableQueueRepository retryDurableQueueRepository,
            RetryDurableDefinition retryDurableDefinition)
        {
            this.logHandler = logHandler;
            this.retryDurableQueueRepository = retryDurableQueueRepository;
            this.retryDurableDefinition = retryDurableDefinition;
        }

        public async Task Invoke(IMessageContext context, MiddlewareDelegate next)
        {

            var key = Encoding.UTF8.GetString((byte[])context.Message.Key);
            try
            {
                var resultAddIfQueueExistsAsync = await this
                    .retryDurableQueueRepository
                    .AddIfQueueExistsAsync(context)
                    .ConfigureAwait(false);

                if (resultAddIfQueueExistsAsync.Status == AddIfQueueExistsResultStatus.Added)
                {
                    return;
                }
            }
            catch (Exception)
            {
                context.ConsumerContext.ShouldStoreOffset = false;
                return;
            }

            var policy = Policy
                .Handle<Exception>(exception => this.retryDurableDefinition.ShouldRetry(new RetryContext(exception)))
                .WaitAndRetryAsync(
                    this.retryDurableDefinition.RetryDurableRetryPlanBeforeDefinition.NumberOfRetries,
                    (retryNumber, c) => this.retryDurableDefinition.RetryDurableRetryPlanBeforeDefinition.TimeBetweenTriesPlan(retryNumber),
                    (exception, waitTime, attemptNumber, c) =>
                    {
                        if (this.retryDurableDefinition.RetryDurableRetryPlanBeforeDefinition.PauseConsumer
                        && !this.controlWorkerId.HasValue)
                        {
                            lock (this.syncPauseAndResume)
                            {
                                if (!this.controlWorkerId.HasValue)
                                {
                                    this.controlWorkerId = context.ConsumerContext.WorkerId;

                                    context.ConsumerContext.Pause();

                                    this.logHandler.Info(
                                        "Consumer paused by retry process",
                                        new
                                        {
                                            ConsumerGroup = context.ConsumerContext.GroupId,
                                            ConsumerName = context.ConsumerContext.ConsumerName,
                                            Worker = context.ConsumerContext.WorkerId
                                        });
                                }
                            }
                        }

                        this.logHandler.Error(
                            $"Exception captured by {nameof(RetryDurableMiddleware)}. Retry in process.",
                            exception,
                            new
                            {
                                AttemptNumber = attemptNumber,
                                WaitMilliseconds = waitTime.TotalMilliseconds,
                                PartitionNumber = context.ConsumerContext.Partition,
                                Worker = context.ConsumerContext.WorkerId,
                                //Headers = context.HeadersAsJson(),
                                //Message = context.Message.ToJson(),
                                ExceptionType = exception.GetType().FullName,
                                ExceptionMessage = exception.Message
                            });
                    }
                );

            try
            {
                await policy
                    .ExecuteAsync(
                        _ => next(context),
                        context.ConsumerContext.WorkerStopped
                    ).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                if (context.ConsumerContext.WorkerStopped.IsCancellationRequested)
                {
                    context.ConsumerContext.ShouldStoreOffset = false;
                }
            }
            catch (Exception exception)
            {
                if (this.retryDurableDefinition.ShouldRetry(new RetryContext(exception)))
                {
                    var resultSaveToQueue = await this
                        .retryDurableQueueRepository
                        .SaveToQueueAsync(context, exception.Message)
                        .ConfigureAwait(false);

                    if (resultSaveToQueue.Status != SaveToQueueResultStatus.Created
                     && resultSaveToQueue.Status != SaveToQueueResultStatus.Added)
                    {
                        context.ConsumerContext.ShouldStoreOffset = false;
                    }
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                if (this.controlWorkerId == context.ConsumerContext.WorkerId)
                {
                    lock (this.syncPauseAndResume)
                    {
                        if (this.controlWorkerId == context.ConsumerContext.WorkerId)
                        {
                            this.controlWorkerId = null;

                            context.ConsumerContext.Resume();

                            this.logHandler.Info(
                                "Consumer resumed by retry process",
                                new
                                {
                                    ConsumerGroup = context.ConsumerContext.GroupId,
                                    ConsumerName = context.ConsumerContext.ConsumerName,
                                    Worker = context.ConsumerContext.WorkerId
                                });
                        }
                    }
                }
            }
        }
    }
}