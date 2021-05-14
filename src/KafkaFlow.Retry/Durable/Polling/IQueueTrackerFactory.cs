namespace KafkaFlow.Retry.Durable.Polling
{
    internal interface IQueueTrackerFactory
    {
        QueueTracker Create();
    }
}