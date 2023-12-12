using System.Threading.Tasks;
using Dawn;

namespace KafkaFlow.Retry.Durable.Polling;

internal class QueueTrackerCoordinator : IQueueTrackerCoordinator
{
    private readonly IQueueTrackerFactory queueTrackerFactory;
    private QueueTracker queueTracker;

    public QueueTrackerCoordinator(IQueueTrackerFactory queueTrackerFactory)
    {
            Guard.Argument(queueTrackerFactory).NotNull();

            this.queueTrackerFactory = queueTrackerFactory;
        }

    public async Task ScheduleJobsAsync(IMessageProducer retryDurableMessageProducer, ILogHandler logHandler)
    {
            queueTracker = queueTrackerFactory
                .Create(retryDurableMessageProducer, logHandler);

            await queueTracker.ScheduleJobsAsync().ConfigureAwait(false);
        }

    public async Task UnscheduleJobsAsync()
    {
            if (queueTracker is object)
            {
                await queueTracker.UnscheduleJobsAsync().ConfigureAwait(false);
            }
        }
}