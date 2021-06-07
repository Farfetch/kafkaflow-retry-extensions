namespace KafkaFlow.Retry
{
    using System;
    using System.Threading.Tasks;
    using KafkaFlow;
    using Polly;

    internal class KafkaRetryMiddleware : IMessageMiddleware
    {
        private readonly KafkaRetryDefinition kafkaRetryDefinition;
        private readonly ILogHandler logHandler;
        private readonly object syncPauseAndResume = new object();
        private int? controlWorkerId;

        public KafkaRetryMiddleware(
            ILogHandler logHandler,
            KafkaRetryDefinition kafkaRetryDefinition)
        {
            this.logHandler = logHandler;
            this.kafkaRetryDefinition = kafkaRetryDefinition;
        }

        public async Task Invoke(IMessageContext context, MiddlewareDelegate next)
        {
            var policy = Policy
                .Handle<Exception>(exception => this.kafkaRetryDefinition.ShouldRetry(new KafkaRetryContext(exception)))
                .WaitAndRetryAsync(
                    this.kafkaRetryDefinition.GetNumberOfRetries(),
                    (retryNumber, c) => this.kafkaRetryDefinition.TimeBetweenTriesPlan(retryNumber),
                    (exception, waitTime, attemptNumber, c) =>
                    {
                        if (this.kafkaRetryDefinition.ShouldPauseConsumer() && !this.controlWorkerId.HasValue)
                        {
                            lock (this.syncPauseAndResume) // TODO: why we need this lock here?
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
                            $"Exception captured by {nameof(KafkaRetryMiddleware)}. Retry in process.",
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
                                //ExceptionMessage = exception.Message
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
            finally
            {
                if (this.controlWorkerId == context.WorkerId) // TODO: understand why this is necessary and the lock below.
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