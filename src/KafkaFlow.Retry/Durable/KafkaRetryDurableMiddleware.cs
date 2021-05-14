namespace KafkaFlow.Retry.Durable
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using KafkaFlow;
    using KafkaFlow.Retry.Durable.Polling;
    using KafkaFlow.Retry.Durable.Repository;
    using KafkaFlow.Retry.Durable.Repository.Actions.Create;
    using Polly;

    internal class KafkaRetryDurableMiddleware : IMessageMiddleware
    {
        private readonly KafkaRetryDurableDefinition kafkaRetryDurableDefinition;
        private readonly ILogHandler logHandler;
        private readonly IQueueTrackerCoordinator queueTrackerCoordinator;
        private readonly IKafkaRetryDurableQueueRepository retryDurableQueueRepository;
        private readonly object syncPauseAndResume = new object();
        private bool asd;
        private int? controlWorkerId;

        public KafkaRetryDurableMiddleware(
            ILogHandler logHandler,
            IKafkaRetryDurableQueueRepository retryDurableQueueRepository,
            KafkaRetryDurableDefinition kafkaRetryDurableDefinition,
            IQueueTrackerCoordinator queueTrackerCoordinator)
        {
            this.logHandler = logHandler;
            this.retryDurableQueueRepository = retryDurableQueueRepository;
            this.kafkaRetryDurableDefinition = kafkaRetryDurableDefinition;
            this.queueTrackerCoordinator = queueTrackerCoordinator;
            this.asd = false;
        }

        public async Task Invoke(IMessageContext context, MiddlewareDelegate next)
        {
            // Where should find the cancellation token
            if (!this.asd)
            {
                await this.queueTrackerCoordinator.InitializeAsync(kafkaRetryDurableDefinition, CancellationToken.None).ConfigureAwait(false);
                this.asd = true;
            }

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
                context.Consumer.ShouldStoreOffset = false;
                return;
            }

            var policy = Policy
                .Handle<Exception>(exception => this.kafkaRetryDurableDefinition.ShouldRetry(new KafkaRetryContext(exception)))
                .WaitAndRetryAsync(
                    this.kafkaRetryDurableDefinition.KafkaRetryDurableRetryPlanBeforeDefinition.GetNumberOfRetries(),
                    (retryNumber, c) => this.kafkaRetryDurableDefinition.KafkaRetryDurableRetryPlanBeforeDefinition.TimeBetweenTriesPlan(retryNumber),
                    (exception, waitTime, attemptNumber, c) =>
                    {
                        if (this.kafkaRetryDurableDefinition.KafkaRetryDurableRetryPlanBeforeDefinition.ShouldPauseConsumer()
                        && !this.controlWorkerId.HasValue)
                        {
                            lock (this.syncPauseAndResume)
                            {
                                if (!this.controlWorkerId.HasValue)
                                {
                                    this.controlWorkerId = context.WorkerId;

                                    context.Consumer.Pause();

                                    this.logHandler.Info(
                                        "Consumer paused by retry process",
                                        new
                                        {
                                            ConsumerGroup = context.GroupId,
                                            ConsumerName = context.Consumer.Name,
                                            Worker = context.WorkerId
                                        });
                                }
                            }
                        }

                        this.logHandler.Error(
                            $"Exception captured by {nameof(KafkaRetryDurableMiddleware)}. Retry in process.",
                            exception,
                            new
                            {
                                AttemptNumber = attemptNumber,
                                WaitMilliseconds = waitTime.TotalMilliseconds,
                                PartitionNumber = context.Partition,
                                Worker = context.WorkerId,
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
                        context.Consumer.WorkerStopped
                    ).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                if (context.Consumer.WorkerStopped.IsCancellationRequested)
                {
                    context.Consumer.ShouldStoreOffset = false;
                }
            }
            catch (Exception exception)
            {
                if (this.kafkaRetryDurableDefinition.ShouldRetry(new KafkaRetryContext(exception)))
                {
                    var resultSaveToQueue = await this
                        .retryDurableQueueRepository
                        .SaveToQueueAsync(context, exception.Message)
                        .ConfigureAwait(false);

                    if (resultSaveToQueue.Status != SaveToQueueResultStatus.Created
                     && resultSaveToQueue.Status != SaveToQueueResultStatus.Added)
                    {
                        context.Consumer.ShouldStoreOffset = false;
                    }
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                if (this.controlWorkerId == context.WorkerId)
                {
                    lock (this.syncPauseAndResume)
                    {
                        if (this.controlWorkerId == context.WorkerId)
                        {
                            this.controlWorkerId = null;

                            context.Consumer.Resume();

                            this.logHandler.Info(
                                "Consumer resumed by retry process",
                                new
                                {
                                    ConsumerGroup = context.GroupId,
                                    ConsumerName = context.Consumer.Name,
                                    Worker = context.WorkerId
                                });
                        }
                    }
                }
            }
        }
    }
}