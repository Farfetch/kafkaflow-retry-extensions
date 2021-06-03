namespace KafkaFlow.Retry
{
    using System;
    using System.Collections.Generic;
    using KafkaFlow;
    using KafkaFlow.Configuration;
    using KafkaFlow.Producers;
    using KafkaFlow.Retry.Durable;
    using KafkaFlow.Retry.Durable.Polling;
    using KafkaFlow.Retry.Durable.Repository;
    using KafkaFlow.Retry.Durable.Repository.Adapters;

    public class KafkaRetryDurableDefinitionBuilder
    {
        private readonly List<Func<KafkaRetryContext, bool>> retryWhenExceptions = new List<Func<KafkaRetryContext, bool>>();
        private IDependencyConfigurator dependencyConfigurator;
        private IKafkaRetryDurableQueueRepository durableQueueRepository;
        private KafkaRetryDurableEmbeddedClusterDefinitionBuilder kafkaRetryDurableEmbeddedClusterBuilder;
        private KafkaRetryDurablePollingDefinition kafkaRetryDurablePollingDefinition;
        private IKafkaRetryDurableQueueRepositoryProvider KafkaRetryDurableRepositoryProvider;
        private KafkaRetryDurableRetryPlanBeforeDefinition kafkaRetryDurableRetryPlanBeforeDefinition;

        public KafkaRetryDurableDefinitionBuilder(IDependencyConfigurator dependencyConfigurator)
        {
            this.dependencyConfigurator = dependencyConfigurator;
        }

        public KafkaRetryDurableDefinitionBuilder Handle<TException>()
            where TException : Exception
            => this.Handle(kafkaRetryContext => kafkaRetryContext.Exception is TException);

        public KafkaRetryDurableDefinitionBuilder Handle<TException>(Func<TException, bool> rule)
            where TException : Exception
            => this.Handle(context => context.Exception is TException ex && rule(ex));

        public KafkaRetryDurableDefinitionBuilder Handle(Func<KafkaRetryContext, bool> func)
        {
            this.retryWhenExceptions.Add(func);
            return this;
        }

        public KafkaRetryDurableDefinitionBuilder HandleAnyException()
            => this.Handle(kafkaRetryContext => true);

        public KafkaRetryDurableDefinitionBuilder WithEmbeddedRetryCluster(
            IClusterConfigurationBuilder cluster,
            Action<KafkaRetryDurableEmbeddedClusterDefinitionBuilder> configure
            )
        {
            this.kafkaRetryDurableEmbeddedClusterBuilder = new KafkaRetryDurableEmbeddedClusterDefinitionBuilder(cluster);
            configure(this.kafkaRetryDurableEmbeddedClusterBuilder);
            this.kafkaRetryDurableEmbeddedClusterBuilder.Build();

            return this;
        }

        public KafkaRetryDurableDefinitionBuilder WithPollingConfiguration(Action<KafkaRetryDurablePollingDefinitionBuilder> configure)
        {
            var kafkaRetryDurablePollingDefinitionBuilder = new KafkaRetryDurablePollingDefinitionBuilder();
            configure(kafkaRetryDurablePollingDefinitionBuilder);
            this.kafkaRetryDurablePollingDefinition = kafkaRetryDurablePollingDefinitionBuilder.Build();

            return this;
        }

        public KafkaRetryDurableDefinitionBuilder WithRepositoryProvider(IKafkaRetryDurableQueueRepositoryProvider kafkaRetryDurableRepositoryProvider)
        {
            this.KafkaRetryDurableRepositoryProvider = kafkaRetryDurableRepositoryProvider;

            return this;
        }

        public KafkaRetryDurableDefinitionBuilder WithRetryPlanBeforeRetryDurable(Action<KafkaRetryDurableRetryPlanBeforeDefinitionBuilder> configure)
        {
            var kafkaRetryDurableRetryPlanBeforeDefinitionBuilder = new KafkaRetryDurableRetryPlanBeforeDefinitionBuilder();
            configure(kafkaRetryDurableRetryPlanBeforeDefinitionBuilder);
            this.kafkaRetryDurableRetryPlanBeforeDefinition = kafkaRetryDurableRetryPlanBeforeDefinitionBuilder.Build();

            return this;
        }

        internal KafkaRetryDurableDefinition Build()
        {
            var kafkaRetryDurableDefinition =
                   new KafkaRetryDurableDefinition(
                       this.retryWhenExceptions,
                       this.kafkaRetryDurableRetryPlanBeforeDefinition,
                       this.kafkaRetryDurablePollingDefinition
                   );

            this.dependencyConfigurator
                .AddSingleton<IKafkaRetryDurableQueueRepository>(
                    service =>
                        new KafkaRetryDurableQueueRepository(
                            this.KafkaRetryDurableRepositoryProvider,
                            new IUpdateRetryQueueItemHandler[]
                            {
                                new UpdateRetryQueueItemStatusHandler(this.KafkaRetryDurableRepositoryProvider),
                                new UpdateRetryQueueItemExecutionInfoHandler(this.KafkaRetryDurableRepositoryProvider)
                            },
                            new HeadersAdapter(),
                            this.kafkaRetryDurablePollingDefinition));

            this.dependencyConfigurator
                .AddSingleton<IQueueTrackerCoordinator>(
                    resolver =>
                        new QueueTrackerCoordinator(
                            new QueueTrackerFactory(
                                resolver.Resolve<IKafkaRetryDurableQueueRepository>(),
                                resolver.Resolve<IProducerAccessor>().GetProducer(KafkaRetryDurableConstants.EmbeddedProducerName),
                                this.kafkaRetryDurablePollingDefinition
                            ),
                            this.kafkaRetryDurablePollingDefinition
                        )
                );

            return kafkaRetryDurableDefinition;
        }
    }
}