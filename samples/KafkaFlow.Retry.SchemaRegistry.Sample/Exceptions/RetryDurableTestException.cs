using System;

namespace KafkaFlow.Retry.SchemaRegistry.Sample.Exceptions;

public class RetryDurableTestException : Exception
{
    public RetryDurableTestException(string message) : base(message)
    { }
}