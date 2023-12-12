using System;
using System.Threading.Tasks;
using Polly;

namespace KafkaFlow.Retry.Simple;

internal class RetrySimpleMiddleware : IMessageMiddleware
{
    private readonly ILogHandler _logHandler;
    private readonly RetrySimpleDefinition _retrySimpleDefinition;
    private readonly object _syncPauseAndResume = new();
    private int? _controlWorkerId;

    public RetrySimpleMiddleware(
        ILogHandler logHandler,
        RetrySimpleDefinition retrySimpleDefinition)
    {
        _logHandler = logHandler;
        _retrySimpleDefinition = retrySimpleDefinition;
    }

    public async Task Invoke(IMessageContext context, MiddlewareDelegate next)
    {
        var policy = Policy
            .Handle<Exception>(exception => _retrySimpleDefinition.ShouldRetry(new RetryContext(exception)))
            .WaitAndRetryAsync(
                _retrySimpleDefinition.NumberOfRetries,
                (retryNumber, c) => _retrySimpleDefinition.TimeBetweenTriesPlan(retryNumber),
                (exception, waitTime, attemptNumber, c) =>
                {
                    if (_retrySimpleDefinition.PauseConsumer && !_controlWorkerId.HasValue)
                    {
                        lock (_syncPauseAndResume) // TODO: why we need this lock here?
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
            if (_controlWorkerId ==
                context.ConsumerContext.WorkerId) // TODO: understand why this is necessary and the lock below.
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