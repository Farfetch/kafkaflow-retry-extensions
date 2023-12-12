using System;

namespace KafkaFlow.Retry.Sample.Exceptions;

public class RetryDurableTestException : Exception
{
    public RetryDurableTestException(string message) : base(message)
    {
    }
}