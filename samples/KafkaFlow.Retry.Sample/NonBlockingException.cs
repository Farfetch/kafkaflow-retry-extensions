using System;

namespace KafkaFlow.Retry.Sample
{
    public class NonBlockingException : Exception
    {
        public NonBlockingException(string message) : base(message)
        {
        }
    }
}