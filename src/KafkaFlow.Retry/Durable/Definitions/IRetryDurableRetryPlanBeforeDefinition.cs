namespace KafkaFlow.Retry.Durable.Definitions
{
    using System;

    internal interface IRetryDurableRetryPlanBeforeDefinition
    {
        int NumberOfRetries { get; }

        bool PauseConsumer { get; }

        Func<int, TimeSpan> TimeBetweenTriesPlan { get; }
    }
}