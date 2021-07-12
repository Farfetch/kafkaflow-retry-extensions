namespace KafkaFlow.Retry.Simple
{
    using System;

    internal interface IRetrySimpleDefinition
    {
        int NumberOfRetries { get; }

        bool PauseConsumer { get; }

        Func<int, TimeSpan> TimeBetweenTriesPlan { get; }

        bool ShouldRetry(RetryContext kafkaRetryContext);
    }
}