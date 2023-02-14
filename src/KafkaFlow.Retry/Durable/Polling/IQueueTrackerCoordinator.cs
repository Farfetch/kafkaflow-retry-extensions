namespace KafkaFlow.Retry.Durable.Polling
{
    internal interface IQueueTrackerCoordinator
    {
        void ScheduleJobs(IMessageProducer retryDurableMessageProducer, ILogHandler logHandler);

        void UnscheduleJobs();
    }
}