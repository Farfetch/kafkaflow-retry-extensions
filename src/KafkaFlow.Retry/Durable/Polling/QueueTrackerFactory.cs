namespace KafkaFlow.Retry.Durable.Polling
{
    using System.Linq;
    using Dawn;
    using KafkaFlow.Retry.Durable.Definitions.Polling;
    using KafkaFlow.Retry.Durable.Encoders;
    using KafkaFlow.Retry.Durable.Polling.Jobs;
    using KafkaFlow.Retry.Durable.Repository;
    using KafkaFlow.Retry.Durable.Repository.Adapters;

    internal class QueueTrackerFactory : IQueueTrackerFactory
    {
        private readonly IMessageAdapter messageAdapter;
        private readonly IMessageHeadersAdapter messageHeadersAdapter;
        private readonly PollingDefinitionsAggregator pollingDefinitionsAggregator;
        private readonly IRetryDurableQueueRepository retryDurableQueueRepository;
        private readonly IUtf8Encoder utf8Encoder;

        public QueueTrackerFactory(
            PollingDefinitionsAggregator pollingDefinitionsAggregator,
            IRetryDurableQueueRepository retryDurableQueueRepository,
            IMessageHeadersAdapter messageHeadersAdapter,
            IMessageAdapter messageAdapter,
            IUtf8Encoder utf8Encoder
        )
        {
            Guard.Argument(pollingDefinitionsAggregator, nameof(pollingDefinitionsAggregator)).NotNull();
            Guard.Argument(retryDurableQueueRepository).NotNull();
            Guard.Argument(messageHeadersAdapter).NotNull();
            Guard.Argument(messageAdapter).NotNull();
            Guard.Argument(utf8Encoder).NotNull();

            this.pollingDefinitionsAggregator = pollingDefinitionsAggregator;
            this.retryDurableQueueRepository = retryDurableQueueRepository;
            this.messageHeadersAdapter = messageHeadersAdapter;
            this.messageAdapter = messageAdapter;
            this.utf8Encoder = utf8Encoder;
        }

        public QueueTracker Create(IMessageProducer retryDurableMessageProducer, ILogHandler logHandler)
        {
            var jobDataProviders = new IJobDataProvider[]
            {
                new RetryDurableJobDataProvider(
                    this.retryDurableQueueRepository,
                    logHandler,
                    this.messageHeadersAdapter,
                    this.messageAdapter,
                    this.utf8Encoder,
                    retryDurableMessageProducer,
                    this.GetPollingDefinition<RetryDurablePollingDefinition>(PollingJobType.RetryDurable),
                    this.pollingDefinitionsAggregator.SchedulerId),

                new CleanupJobDataProvider(
                    this.retryDurableQueueRepository,
                    logHandler,
                    this.GetPollingDefinition<CleanupPollingDefinition>(PollingJobType.Cleanup),
                    this.pollingDefinitionsAggregator.SchedulerId)
            };

            return new QueueTracker(
                logHandler,
                this.pollingDefinitionsAggregator.SchedulerId,
                jobDataProviders,
                new TriggerProvider());
        }

        private TPollingDefinition GetPollingDefinition<TPollingDefinition>(PollingJobType pollingJobType) where TPollingDefinition : PollingDefinition
        {
            var pollingDefinitions = this.pollingDefinitionsAggregator.PollingDefinitions;

            Guard.Argument(pollingDefinitions.Keys, nameof(pollingDefinitions.Keys)).Contains(pollingJobType);

            var pollingDefinition = pollingDefinitions[pollingJobType];

            Guard.Argument(pollingDefinition, nameof(pollingDefinition)).NotNull().Compatible<TPollingDefinition>();

            return pollingDefinition as TPollingDefinition;
        }
    }
}