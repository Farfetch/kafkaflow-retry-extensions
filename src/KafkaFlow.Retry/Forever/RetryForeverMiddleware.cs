using System;
using System.Threading.Tasks;
using Polly;

namespace KafkaFlow.Retry.Forever;

internal class RetryForeverMiddleware : IMessageMiddleware
{
    private readonly ILogHandler _logHandler;
    private readonly RetryForeverDefinition _retryForeverDefinition;
    private readonly object _syncPauseAndResume = new();
    private int? _controlWorkerId;

    public RetryForeverMiddleware(
        ILogHandler logHandler,
        RetryForeverDefinition retryForeverDefinition)
    {
        _logHandler = logHandler;
        _retryForeverDefinition = retryForeverDefinition;
    }

    public async Task Invoke(IMessageContext context, MiddlewareDelegate next)
    {
        var policy = Policy
            .Handle<Exception>(exception => _retryForeverDefinition.ShouldRetry(new RetryContext(exception)))
            .WaitAndRetryForeverAsync(
                (retryNumber, _) => _retryForeverDefinition.TimeBetweenTriesPlan(retryNumber),
                (exception, attemptNumber, waitTime, _) =>
                {
                    if (!_controlWorkerId.HasValue)
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
                        $"Exception captured by {nameof(RetryForeverMiddleware)}. Retry in process.",
                        exception,
                        new
                        {
                            AttemptNumber = attemptNumber,
                            WaitMilliseconds = waitTime.TotalMilliseconds,
                            PartitionNumber = context.ConsumerContext.Partition,
                            Worker = context.ConsumerContext.WorkerId,
                            Offset = context.ConsumerContext.Offset,
                            //Headers = context.HeadersAsJson(),
                            //Message = context.Message.ToJson(),
                            ExceptionType = exception.GetType().FullName
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
