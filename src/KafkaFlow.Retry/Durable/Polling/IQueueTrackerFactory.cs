namespace KafkaFlow.Retry.Durable.Polling
{
    using KafkaFlow.Retry.Durable.Definitions;

    internal interface IQueueTrackerFactory
    {
        QueueTracker Create(
            RetryDurablePollingDefinition retryDurablePollingDefinition,
            IMessageProducer retryDurableMessageProducer,
            ILogHandler logHandler);
    }
}