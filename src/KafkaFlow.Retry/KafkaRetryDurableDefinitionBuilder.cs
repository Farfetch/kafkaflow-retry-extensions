namespace KafkaFlow.Retry
{
    using System;
    using System.Collections.Generic;
    using KafkaFlow;
    using KafkaFlow.Configuration;
    using KafkaFlow.Consumers;
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
        private KafkaRetryDurablePollingDefinition kafkaRetryDurablePollingDefinition;
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
            var kafkaRetryDurableEmbeddedClusterBuilder = new KafkaRetryDurableEmbeddedClusterDefinitionBuilder(cluster);
            configure(kafkaRetryDurableEmbeddedClusterBuilder);
            kafkaRetryDurableEmbeddedClusterBuilder.Build();
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
            this.dependencyConfigurator
                .AddSingleton<IKafkaRetryDurableQueueRepository>(
                    service =>
                        new KafkaRetryDurableQueueRepository(
                            kafkaRetryDurableRepositoryProvider,
                            new IUpdateRetryQueueItemHandler[]
                            {
                                new UpdateRetryQueueItemStatusHandler(kafkaRetryDurableRepositoryProvider),
                                new UpdateRetryQueueItemExecutionInfoHandler(kafkaRetryDurableRepositoryProvider)
                            },
                            new HeadersAdapter()));

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
            this.dependencyConfigurator
                .AddSingleton<IQueueTrackerCoordinator>(
                service =>
                    new QueueTrackerCoordinator(
                        new QueueTrackerFactory(
                            service.Resolve<IKafkaRetryDurableQueueRepository>(),
                            service.Resolve<IProducerAccessor>().GetProducer(KafkaRetryDurableConstants.EmbeddedProducerName),
                            service.Resolve<IConsumerAccessor>().GetConsumer(KafkaRetryDurableConstants.EmbeddedConsumerName)
                        ),
                        service.Resolve<IConsumerAccessor>()
                    )
                );

            var kafkaRetryDurableDefinition =
                new KafkaRetryDurableDefinition(
                    this.retryWhenExceptions,
                    this.kafkaRetryDurableRetryPlanBeforeDefinition,
                    this.kafkaRetryDurablePollingDefinition
                );

            return kafkaRetryDurableDefinition;
        }
    }
}