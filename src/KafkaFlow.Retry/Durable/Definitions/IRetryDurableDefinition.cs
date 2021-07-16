namespace KafkaFlow.Retry.Durable.Definitions
{
    internal interface IRetryDurableDefinition
    {
        IRetryDurablePollingDefinition RetryDurablePollingDefinition { get; }

        IRetryDurableRetryPlanBeforeDefinition RetryDurableRetryPlanBeforeDefinition { get; }

        bool ShouldRetry(RetryContext kafkaRetryContext);
    }
}