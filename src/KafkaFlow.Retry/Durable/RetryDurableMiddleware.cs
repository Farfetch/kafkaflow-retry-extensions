using System;
using System.Threading.Tasks;
using Dawn;
using KafkaFlow.Retry.Durable.Definitions;
using KafkaFlow.Retry.Durable.Repository.Actions.Create;
using Polly;

namespace KafkaFlow.Retry.Durable;

internal class RetryDurableMiddleware : IMessageMiddleware
{
    private readonly ILogHandler _logHandler;
    private readonly RetryDurableDefinition _retryDurableDefinition;
    private readonly object _syncPauseAndResume = new();
    private int? _controlWorkerId;

    public RetryDurableMiddleware(
        ILogHandler logHandler,
        RetryDurableDefinition retryDurableDefinition)
    {
        Guard.Argument(logHandler).NotNull();
        Guard.Argument(retryDurableDefinition).NotNull();

        _logHandler = logHandler;
        _retryDurableDefinition = retryDurableDefinition;
    }

    public async Task Invoke(IMessageContext context, MiddlewareDelegate next)
    {
        try
        {
            var resultAddIfQueueExistsAsync = await _retryDurableDefinition
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
            .Handle<Exception>(exception => _retryDurableDefinition.ShouldRetry(new RetryContext(exception)))
            .WaitAndRetryAsync(
                _retryDurableDefinition.RetryDurableRetryPlanBeforeDefinition.NumberOfRetries,
                (retryNumber, _) =>
                    _retryDurableDefinition.RetryDurableRetryPlanBeforeDefinition.TimeBetweenTriesPlan(retryNumber),
                (exception, waitTime, attemptNumber, c) =>
                {
                    if (_retryDurableDefinition.RetryDurableRetryPlanBeforeDefinition.PauseConsumer
                        && !_controlWorkerId.HasValue)
                    {
                        lock (_syncPauseAndResume)
                        {
                            if (!_controlWorkerId.HasValue)
                            {
                                _controlWorkerId = context.ConsumerContext.WorkerId;

                                context.ConsumerContext.Pause();

                                _logHandler.Info(
                                    "Consumer paused by retry process",
                                    new
                                    {
                                        ConsumerGroup = context.ConsumerContext.GroupId,
                                        context.ConsumerContext.ConsumerName,
                                        Worker = context.ConsumerContext.WorkerId
                                    });
                            }
                        }
                    }

                    _logHandler.Error(
                        $"Exception captured by {nameof(RetryDurableMiddleware)}. Retry in process.",
                        exception,
                        new
                        {
                            AttemptNumber = attemptNumber,
                            WaitMilliseconds = waitTime.TotalMilliseconds,
                            PartitionNumber = context.ConsumerContext.Partition,
                            Worker = context.ConsumerContext.WorkerId,
                            Offset = context.ConsumerContext.Offset,
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
            if (_retryDurableDefinition.ShouldRetry(new RetryContext(exception)))
            {
                var resultSaveToQueue = await _retryDurableDefinition
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
            if (_controlWorkerId == context.ConsumerContext.WorkerId)
            {
                lock (_syncPauseAndResume)
                {
                    if (_controlWorkerId == context.ConsumerContext.WorkerId)
                    {
                        _controlWorkerId = null;

                        context.ConsumerContext.Resume();

                        _logHandler.Info(
                            "Consumer resumed by retry process",
                            new
                            {
                                ConsumerGroup = context.ConsumerContext.GroupId,
                                context.ConsumerContext.ConsumerName,
                                Worker = context.ConsumerContext.WorkerId
                            });
                    }
                }
            }
        }
    }
}
