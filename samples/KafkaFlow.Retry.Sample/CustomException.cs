using System;

namespace KafkaFlow.Retry.Sample
{
    public class CustomException : Exception
    {
        public CustomException(string message) : base(message)
        {
        }
    }
}