namespace KafkaFlow.Retry.Sample.Exceptions
{
    using System;

    public class RetryForeverTestException : Exception
    {
        public RetryForeverTestException(string message) : base(message)
        { }
    }
}