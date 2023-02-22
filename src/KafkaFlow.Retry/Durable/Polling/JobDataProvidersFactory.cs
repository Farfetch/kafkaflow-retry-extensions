namespace KafkaFlow.Retry.Durable.Polling
{
    using System.Collections.Generic;
    using Dawn;
    using KafkaFlow.Retry.Durable.Definitions.Polling;
    using KafkaFlow.Retry.Durable.Encoders;
    using KafkaFlow.Retry.Durable.Polling.Jobs;
    using KafkaFlow.Retry.Durable.Repository;
    using KafkaFlow.Retry.Durable.Repository.Adapters;
    using Quartz;

    internal class JobDataProvidersFactory : IJobDataProvidersFactory
    {
        private readonly IMessageAdapter messageAdapter;
        private readonly IMessageHeadersAdapter messageHeadersAdapter;
        private readonly PollingDefinitionsAggregator pollingDefinitionsAggregator;
        private readonly IRetryDurableQueueRepository retryDurableQueueRepository;
        private readonly ITriggerProvider triggerProvider;
        private readonly IUtf8Encoder utf8Encoder;

        public JobDataProvidersFactory(
            PollingDefinitionsAggregator pollingDefinitionsAggregator,
            ITriggerProvider triggerProvider,
            IRetryDurableQueueRepository retryDurableQueueRepository,
            IMessageHeadersAdapter messageHeadersAdapter,
            IMessageAdapter messageAdapter,
            IUtf8Encoder utf8Encoder)
        {
            Guard.Argument(pollingDefinitionsAggregator, nameof(pollingDefinitionsAggregator)).NotNull();
            Guard.Argument(triggerProvider, nameof(triggerProvider)).NotNull();
            Guard.Argument(retryDurableQueueRepository).NotNull();
            Guard.Argument(messageHeadersAdapter).NotNull();
            Guard.Argument(messageAdapter).NotNull();
            Guard.Argument(utf8Encoder).NotNull();

            this.pollingDefinitionsAggregator = pollingDefinitionsAggregator;
            this.retryDurableQueueRepository = retryDurableQueueRepository;
            this.messageHeadersAdapter = messageHeadersAdapter;
            this.messageAdapter = messageAdapter;
            this.utf8Encoder = utf8Encoder;
            this.triggerProvider = triggerProvider;
        }

        public IEnumerable<IJobDataProvider> Create(IMessageProducer retryDurableMessageProducer, ILogHandler logHandler)
        {
            var jobDataProviders = new List<IJobDataProvider>(2);

            if (this.TryGetPollingDefinition<RetryDurablePollingDefinition>(PollingJobType.RetryDurable, out var retryDurablePollingDefinition))
            {
                jobDataProviders.Add(
                    new RetryDurableJobDataProvider(
                        retryDurablePollingDefinition,
                        this.GetTrigger(retryDurablePollingDefinition),
                        this.pollingDefinitionsAggregator.SchedulerId,
                        this.retryDurableQueueRepository,
                        logHandler,
                        this.messageHeadersAdapter,
                        this.messageAdapter,
                        this.utf8Encoder,
                        retryDurableMessageProducer
                        )
                    );
            }

            if (this.TryGetPollingDefinition<CleanupPollingDefinition>(PollingJobType.Cleanup, out var cleanupPollingDefinition))
            {
                jobDataProviders.Add(
                    new CleanupJobDataProvider(
                        cleanupPollingDefinition,
                        this.GetTrigger(cleanupPollingDefinition),
                        this.pollingDefinitionsAggregator.SchedulerId,
                        this.retryDurableQueueRepository,
                        logHandler
                        )
                    );
            }

            return jobDataProviders;
        }

        private ITrigger GetTrigger(PollingDefinition pollingDefinition)
        {
            return this.triggerProvider.GetPollingTrigger(this.pollingDefinitionsAggregator.SchedulerId, pollingDefinition);
        }

        private bool TryGetPollingDefinition<TPollingDefinition>(PollingJobType pollingJobType, out TPollingDefinition pollingDefinition) where TPollingDefinition : PollingDefinition
        {
            pollingDefinition = default;

            var pollingDefinitions = this.pollingDefinitionsAggregator.PollingDefinitions;

            var pollingDefinitionFound = pollingDefinitions.TryGetValue(pollingJobType, out var pollingDefinitionResult);

            if (pollingDefinitionFound)
            {
                Guard.Argument(pollingDefinitionResult, nameof(pollingDefinitionResult)).NotNull().Compatible<TPollingDefinition>();
                pollingDefinition = pollingDefinitionResult as TPollingDefinition;
            }

            return pollingDefinitionFound;
        }
    }
}