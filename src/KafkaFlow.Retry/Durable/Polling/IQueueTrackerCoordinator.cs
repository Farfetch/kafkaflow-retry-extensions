namespace KafkaFlow.Retry.Durable.Polling
{
    internal interface IQueueTrackerCoordinator
    {
        void Initialize();

        void Shutdown();
    }
}