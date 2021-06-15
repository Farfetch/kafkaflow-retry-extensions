namespace KafkaFlow.Retry.Durable.Definitions
{
    internal interface IRetryDurableDefinition
    {
        RetryDurablePollingDefinition RetryDurablePollingDefinition { get; }

        RetryDurableRetryPlanBeforeDefinition RetryDurableRetryPlanBeforeDefinition { get; }

        bool ShouldRetry(RetryContext kafkaRetryContext);
    }
}