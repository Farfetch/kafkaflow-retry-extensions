namespace KafkaFlow.Retry.Sample.Exceptions
{
    using System;

    internal class RetrySimpleTestException : Exception
    {
        public RetrySimpleTestException(string message) : base(message)
        { }
    }
}