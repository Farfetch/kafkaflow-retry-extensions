using System;

namespace KafkaFlow.Retry.Sample.Exceptions;

public class RetryForeverTestException : Exception
{
    public RetryForeverTestException(string message) : base(message)
    { }
}