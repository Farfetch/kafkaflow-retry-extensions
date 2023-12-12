using System;
using System.Threading.Tasks;
using Dawn;
using KafkaFlow;
using KafkaFlow.Retry.Durable.Definitions;
using KafkaFlow.Retry.Durable.Repository.Actions.Create;
using Polly;

namespace KafkaFlow.Retry.Durable;

internal class RetryDurableMiddleware : IMessageMiddleware
{
    private readonly ILogHandler logHandler;
    private readonly RetryDurableDefinition retryDurableDefinition;
    private readonly object syncPauseAndResume = new object();
    private int? controlWorkerId;

    public RetryDurableMiddleware(
        ILogHandler logHandler,
        RetryDurableDefinition retryDurableDefinition)
    {
            Guard.Argument(logHandler).NotNull();
            Guard.Argument(retryDurableDefinition).NotNull();

            this.logHandler = logHandler;
            this.retryDurableDefinition = retryDurableDefinition;
        }

    public async Task Invoke(IMessageContext context, MiddlewareDelegate next)
    {
            try
            {
                var resultAddIfQueueExistsAsync = await retryDurableDefinition
                    .RetryDurableQueueRepository
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
                .Handle<Exception>(exception => retryDurableDefinition.ShouldRetry(new RetryContext(exception)))
                .WaitAndRetryAsync(
                    retryDurableDefinition.RetryDurableRetryPlanBeforeDefinition.NumberOfRetries,
                    (retryNumber, c) => retryDurableDefinition.RetryDurableRetryPlanBeforeDefinition.TimeBetweenTriesPlan(retryNumber),
                    (exception, waitTime, attemptNumber, c) =>
                    {
                        if (retryDurableDefinition.RetryDurableRetryPlanBeforeDefinition.PauseConsumer
                        && !controlWorkerId.HasValue)
                        {
                            lock (syncPauseAndResume)
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
                            $"Exception captured by {nameof(RetryDurableMiddleware)}. Retry in process.",
                            exception,
                            new
                            {
                                AttemptNumber = attemptNumber,
                                WaitMilliseconds = waitTime.TotalMilliseconds,
                                PartitionNumber = context.ConsumerContext.Partition,
                                Worker = context.ConsumerContext.WorkerId,
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
                if (retryDurableDefinition.ShouldRetry(new RetryContext(exception)))
                {
                    var resultSaveToQueue = await retryDurableDefinition
                        .RetryDurableQueueRepository
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
                if (controlWorkerId == context.ConsumerContext.WorkerId)
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