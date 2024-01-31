using System.Threading.Tasks;
using Dawn;

namespace KafkaFlow.Retry.Durable.Polling;

internal class QueueTrackerCoordinator : IQueueTrackerCoordinator
{
    private readonly IQueueTrackerFactory _queueTrackerFactory;
    private QueueTracker _queueTracker;

    public QueueTrackerCoordinator(IQueueTrackerFactory queueTrackerFactory)
    {
        Guard.Argument(queueTrackerFactory).NotNull();

        _queueTrackerFactory = queueTrackerFactory;
    }

    public async Task ScheduleJobsAsync(IMessageProducer retryDurableMessageProducer, ILogHandler logHandler)
    {
        _queueTracker = _queueTrackerFactory
            .Create(retryDurableMessageProducer, logHandler);

        await _queueTracker.ScheduleJobsAsync().ConfigureAwait(false);
    }

    public async Task UnscheduleJobsAsync()
    {
        if (_queueTracker is object)
        {
            await _queueTracker.UnscheduleJobsAsync().ConfigureAwait(false);
        }
    }
}