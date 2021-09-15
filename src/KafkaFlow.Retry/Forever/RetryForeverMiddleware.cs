namespace KafkaFlow.Retry.Forever
{
    using System;
    using System.Threading.Tasks;
    using Dawn;
    using KafkaFlow;
    using Polly;

    internal class RetryForeverMiddleware : IMessageMiddleware
    {
        private readonly IRetryForeverDefinition kafkaRetryForeverDefinition;
        private readonly ILogHandler logHandler;
        private readonly object syncPauseAndResume = new object();
        private int? controlWorkerId;

        public RetryForeverMiddleware(
            ILogHandler logHandler,
            IRetryForeverDefinition kafkaRetryForeverDefinition)
        {
            Guard.Argument(logHandler).NotNull();
            Guard.Argument(kafkaRetryForeverDefinition).NotNull();

            this.logHandler = logHandler;
            this.kafkaRetryForeverDefinition = kafkaRetryForeverDefinition;
        }

        public async Task Invoke(IMessageContext context, MiddlewareDelegate next)
        {
            var policy = Policy
                .Handle<Exception>(exception => this.kafkaRetryForeverDefinition.ShouldRetry(new RetryContext(exception)))
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
                            $"Exception captured by {nameof(RetryForeverMiddleware)}. Retry in process.",
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