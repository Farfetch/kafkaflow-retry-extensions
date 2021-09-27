namespace KafkaFlow.Retry.Sample.Exceptions
{
    using System;

    internal class RetryForeverTestException : Exception
    {
        public RetryForeverTestException(string message) : base(message)
        { }
    }
}