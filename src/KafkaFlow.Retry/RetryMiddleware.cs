namespace KafkaFlow.Retry
{
    using System;
    using System.Threading.Tasks;
    using KafkaFlow;
    using Polly;

    internal class RetryMiddleware : IMessageMiddleware
    {
        private readonly RetryDefinition kafkaRetryDefinition;
        private readonly ILogHandler logHandler;
        private readonly object syncPauseAndResume = new object();
        private int? controlWorkerId;

        public RetryMiddleware(
            ILogHandler logHandler,
            RetryDefinition kafkaRetryDefinition)
        {
            this.logHandler = logHandler;
            this.kafkaRetryDefinition = kafkaRetryDefinition;
        }

        public async Task Invoke(IMessageContext context, MiddlewareDelegate next)
        {
            var policy = Policy
                .Handle<Exception>(exception => this.kafkaRetryDefinition.ShouldRetry(new RetryContext(exception)))
                .WaitAndRetryAsync(
                    this.kafkaRetryDefinition.NumberOfRetries,
                    (retryNumber, c) => this.kafkaRetryDefinition.TimeBetweenTriesPlan(retryNumber),
                    (exception, waitTime, attemptNumber, c) =>
                    {
                        if (this.kafkaRetryDefinition.PauseConsumer && !this.controlWorkerId.HasValue)
                        {
                            lock (this.syncPauseAndResume) // TODO: why we need this lock here?
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
                            $"Exception captured by {nameof(RetryMiddleware)}. Retry in process.",
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
                                //ExceptionMessage = exception.Message
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
            finally
            {
                if (this.controlWorkerId == context.ConsumerContext.WorkerId) // TODO: understand why this is necessary and the lock below.
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