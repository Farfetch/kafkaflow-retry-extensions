using System.Threading.Tasks;

namespace KafkaFlow.Retry.Durable.Polling;

internal interface IQueueTrackerCoordinator
{
    Task ScheduleJobsAsync(IMessageProducer retryDurableMessageProducer, ILogHandler logHandler);

    Task UnscheduleJobsAsync();
}