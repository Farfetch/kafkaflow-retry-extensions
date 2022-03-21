namespace KafkaFlow.Retry.SchemaRegistry.Sample.Exceptions
{
    using System;

    public class RetryDurableTestException : Exception
    {
        public RetryDurableTestException(string message) : base(message)
        { }
    }
}