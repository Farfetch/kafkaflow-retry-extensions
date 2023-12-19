﻿using System;
using Confluent.Kafka;

namespace KafkaFlow.Retry.Durable;

[Serializable]
public class RetryDurableException : Exception
{
    public RetryDurableException(RetryError retryError)
    {
            this.Error = retryError;
        }

    public RetryDurableException(RetryError retryError, string message) : base(message)
    {
            this.Error = retryError;
        }

    public RetryDurableException(RetryError retryError, string message, Exception exception) : base(message, exception)
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