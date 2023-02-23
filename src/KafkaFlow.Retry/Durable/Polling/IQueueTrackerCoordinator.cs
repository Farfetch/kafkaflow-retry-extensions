namespace KafkaFlow.Retry.Durable.Polling
{
    using System.Threading.Tasks;

    internal interface IQueueTrackerCoordinator
    {
        Task ScheduleJobsAsync(IMessageProducer retryDurableMessageProducer, ILogHandler logHandler);

        Task UnscheduleJobsAsync();
    }
}