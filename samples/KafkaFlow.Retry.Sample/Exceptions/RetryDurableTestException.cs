namespace KafkaFlow.Retry.Sample.Exceptions
{
    using System;

    internal class RetryDurableTestException : Exception
    {
        public RetryDurableTestException(string message) : base(message)
        { }
    }
}