namespace KafkaFlow.Retry.Durable
{
    using System;
    using Confluent.Kafka;

    [Serializable]
    public class KafkaRetryException : Exception
    {
        public KafkaRetryException(RetryError retryError)
        {
            this.Error = retryError;
        }

        public KafkaRetryException(RetryError retryError, string message) : base(message)
        {
            this.Error = retryError;
        }

        public KafkaRetryException(RetryError retryError, string message, Exception exception) : base(message, exception)
        {
            this.Error = retryError;
            this.KafkaErrorCode = this.GetErrorCode(exception);
        }

        public RetryError Error { get; }
        public ErrorCode KafkaErrorCode { get; }

        public override string ToString()
        {
            string message = $"Kafka Retry Error Code: {Error.Code} | ";
            if (KafkaErrorCode != ErrorCode.Unknown) { message += $"Kafka Error Code: { KafkaErrorCode} | "; }

            return $"{message}{base.ToString()}";
        }

        private ErrorCode GetErrorCode(Exception exception)
        {
            ErrorCode errorCode = ErrorCode.Unknown;

            while (exception is object)
            {
                if (exception is KafkaException)
                {
                    errorCode = ((KafkaException)exception).Error.Code;

                    return errorCode;
                }
                exception = exception.InnerException;
            }

            return errorCode;
        }
    }
}