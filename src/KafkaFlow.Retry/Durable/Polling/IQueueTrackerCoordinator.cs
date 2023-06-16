namespace KafkaFlow.Retry.Durable.Polling
{
    using KafkaFlow.Retry.Durable.Definitions;

    internal interface IQueueTrackerCoordinator
    {
        void ScheduleJob(
            RetryDurablePollingDefinition retryDurablePollingDefinition,
            IMessageProducer retryDurableMessageProducer,
            ILogHandler logHandler);

        void UnscheduleJob();
    }
}