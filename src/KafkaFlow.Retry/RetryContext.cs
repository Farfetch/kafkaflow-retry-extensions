using System;
using Dawn;

namespace KafkaFlow.Retry;

public class RetryContext
{
    public RetryContext(Exception exception)
    {
            Guard.Argument(exception, nameof(exception)).NotNull();

            Exception = exception;
        }

    public Exception Exception { get; }
}