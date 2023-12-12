using System.Collections.Generic;
using Dawn;

namespace KafkaFlow.Retry.Durable.Polling;

internal class QueueTrackerFactory : IQueueTrackerFactory
{
    private readonly IJobDataProvidersFactory _jobDataProvidersFactory;
    private readonly string _schedulerId;
    private IEnumerable<IJobDataProvider> _jobDataProviders;

    public QueueTrackerFactory(
        string schedulerId,
        IJobDataProvidersFactory jobDataProvidersFactory
    )
    {
        Guard.Argument(schedulerId, nameof(schedulerId)).NotNull().NotEmpty();
        Guard.Argument(jobDataProvidersFactory, nameof(jobDataProvidersFactory)).NotNull();

        _schedulerId = schedulerId;
        _jobDataProvidersFactory = jobDataProvidersFactory;
    }

    public QueueTracker Create(IMessageProducer retryDurableMessageProducer, ILogHandler logHandler)
    {
        if (_jobDataProviders is null)
        {
            _jobDataProviders = _jobDataProvidersFactory.Create(retryDurableMessageProducer, logHandler);
        }

        return new QueueTracker(
            _schedulerId,
            _jobDataProviders,
            logHandler);
    }
}