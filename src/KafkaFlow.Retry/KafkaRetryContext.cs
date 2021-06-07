namespace KafkaFlow.Retry
{
    using System;
    using Dawn;

    public class KafkaRetryContext
    {
        public KafkaRetryContext(Exception exception)
        {
            Guard.Argument(exception, nameof(exception)).NotNull();

            this.Exception = exception;
        }

        public Exception Exception { get; }
    }
}