namespace KafkaFlow.Retry
{
    using System;

    public class KafkaRetryContext
    {
        public KafkaRetryContext(Exception exception)
        {
            this.Exception = exception;
        }

        public Exception Exception { get; }
    }
}