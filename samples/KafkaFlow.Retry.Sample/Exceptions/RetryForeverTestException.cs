namespace KafkaFlow.Retry.Sample.Exceptions
{
    using System;

    public class RetrySimpleTestException : Exception
    {
        public RetrySimpleTestException(string message) : base(message)
        { }
    }
}