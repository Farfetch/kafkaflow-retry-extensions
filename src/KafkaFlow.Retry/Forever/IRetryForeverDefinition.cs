namespace KafkaFlow.Retry.Forever
{
    using System;

    internal interface IRetryForeverDefinition
    {
        Func<int, TimeSpan> TimeBetweenTriesPlan { get; }

        bool ShouldRetry(RetryContext kafkaRetryContext);
    }
}