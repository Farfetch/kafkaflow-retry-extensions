namespace KafkaFlow.Retry.Durable.Polling
{
    using System.Collections.Generic;
    using Dawn;

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
            if (this.jobDataProviders is null)
            {
                this.jobDataProviders = this.jobDataProvidersFactory.Create(retryDurableMessageProducer, logHandler);
            }

            return new QueueTracker(
                this.schedulerId,
                this.jobDataProviders,
                logHandler);
        }
    }
}