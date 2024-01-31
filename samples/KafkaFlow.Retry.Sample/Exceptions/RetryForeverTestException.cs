using System;

namespace KafkaFlow.Retry.Sample.Exceptions;

public class RetrySimpleTestException : Exception
{
    public RetrySimpleTestException(string message) : base(message)
    { }
}