using System.Collections.Generic;
using Dawn;

namespace KafkaFlow.Retry.Durable.Polling;

internal class QueueTrackerFactory : IQueueTrackerFactory
{
    private readonly IJobDataProvidersFactory jobDataProvidersFactory;
    private readonly string schedulerId;
    private IEnumerable<IJobDataProvider> jobDataProviders;

    public QueueTrackerFactory(
        string schedulerId,
        IJobDataProvidersFactory jobDataProvidersFactory
    )
    {
            Guard.Argument(schedulerId, nameof(schedulerId)).NotNull().NotEmpty();
            Guard.Argument(jobDataProvidersFactory, nameof(jobDataProvidersFactory)).NotNull();

            this.schedulerId = schedulerId;
            this.jobDataProvidersFactory = jobDataProvidersFactory;
        }

    public QueueTracker Create(IMessageProducer retryDurableMessageProducer, ILogHandler logHandler)
    {
            if (jobDataProviders is null)
            {
                jobDataProviders = jobDataProvidersFactory.Create(retryDurableMessageProducer, logHandler);
            }

            return new QueueTracker(
                schedulerId,
                jobDataProviders,
                logHandler);
        }
}