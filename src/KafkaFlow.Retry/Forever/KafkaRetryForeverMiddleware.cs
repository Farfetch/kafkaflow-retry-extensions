namespace KafkaFlow.Retry.Forever
{
    using System;
    using System.Threading.Tasks;
    using KafkaFlow;
    using Polly;

    internal class KafkaRetryDurableMiddleware : IMessageMiddleware
    {
        private readonly KafkaRetryDurableDefinition kafkaRetryForeverDefinition;
        private readonly ILogHandler logHandler;
        private readonly object syncPauseAndResume = new object();
        private int? controlWorkerId;

        public KafkaRetryDurableMiddleware(
            ILogHandler logHandler,
            KafkaRetryDurableDefinition kafkaRetryForeverDefinition)
        {
            this.logHandler = logHandler;
            this.kafkaRetryForeverDefinition = kafkaRetryForeverDefinition;
        }

        public async Task Invoke(IMessageContext context, MiddlewareDelegate next)
        {
            var policy = Policy
                .Handle<Exception>(exception => this.kafkaRetryForeverDefinition.ShouldRetry(new KafkaRetryContext(exception)))
                .WaitAndRetryForeverAsync(
                    (retryNumber, c) => this.kafkaRetryForeverDefinition.TimeBetweenTriesPlan(retryNumber),
                    (exception, attemptNumber, waitTime, c) =>
                    {
                        if (!this.controlWorkerId.HasValue)
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