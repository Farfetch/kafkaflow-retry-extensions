using System;
using System.Threading.Tasks;
using KafkaFlow;
using Polly;

namespace KafkaFlow.Retry.Simple;

internal class RetrySimpleMiddleware : IMessageMiddleware
{
    private readonly ILogHandler logHandler;
    private readonly RetrySimpleDefinition retrySimpleDefinition;
    private readonly object syncPauseAndResume = new object();
    private int? controlWorkerId;

    public RetrySimpleMiddleware(
        ILogHandler logHandler,
        RetrySimpleDefinition retrySimpleDefinition)
    {
            this.logHandler = logHandler;
            this.retrySimpleDefinition = retrySimpleDefinition;
        }

    public async Task Invoke(IMessageContext context, MiddlewareDelegate next)
    {
            var policy = Policy
                .Handle<Exception>(exception => retrySimpleDefinition.ShouldRetry(new RetryContext(exception)))
                .WaitAndRetryAsync(
                    retrySimpleDefinition.NumberOfRetries,
                    (retryNumber, c) => retrySimpleDefinition.TimeBetweenTriesPlan(retryNumber),
                    (exception, waitTime, attemptNumber, c) =>
                    {
                        if (retrySimpleDefinition.PauseConsumer && !controlWorkerId.HasValue)
                        {
                            lock (syncPauseAndResume) // TODO: why we need this lock here?
                            {
                                if (!controlWorkerId.HasValue)
                                {
                                    controlWorkerId = context.ConsumerContext.WorkerId;

                                    context.ConsumerContext.Pause();

                                    logHandler.Info(
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

                        logHandler.Error(
                            $"Exception captured by {nameof(RetrySimpleMiddleware)}. Retry in process.",
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
                if (controlWorkerId == context.ConsumerContext.WorkerId) // TODO: understand why this is necessary and the lock below.
                {
                    lock (syncPauseAndResume)
                    {
                        if (controlWorkerId == context.ConsumerContext.WorkerId)
                        {
                            controlWorkerId = null;

                            context.ConsumerContext.Resume();

                            logHandler.Info(
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