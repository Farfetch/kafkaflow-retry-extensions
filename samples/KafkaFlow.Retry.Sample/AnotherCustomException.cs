using System;

namespace KafkaFlow.Retry.Sample
{
    public class AnotherCustomException : Exception
    {
        public AnotherCustomException(string message) : base(message)
        {
        }
    }
}