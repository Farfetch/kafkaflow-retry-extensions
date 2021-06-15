namespace KafkaFlow.Retry
{
    using System;
    using System.Collections.Generic;
    using KafkaFlow;
    using KafkaFlow.Configuration;
    using KafkaFlow.Producers;
    using KafkaFlow.Retry.Durable;
    using KafkaFlow.Retry.Durable.Compression;
    using KafkaFlow.Retry.Durable.Definitions;
    using KafkaFlow.Retry.Durable.Encoders;
    using KafkaFlow.Retry.Durable.Polling;
    using KafkaFlow.Retry.Durable.Repository;
    using KafkaFlow.Retry.Durable.Repository.Adapters;
    using KafkaFlow.Retry.Durable.Serializers;

    public class RetryDurableDefinitionBuilder
    {
        private readonly IDependencyConfigurator dependencyConfigurator;
        private readonly List<Func<RetryContext, bool>> retryWhenExceptions = new List<Func<RetryContext, bool>>();
        private RetryDurablePollingDefinition retryDurablePollingDefinition;
        private IRetryDurableQueueRepositoryProvider retryDurableRepositoryProvider;
        private RetryDurableRetryPlanBeforeDefinition retryDurableRetryPlanBeforeDefinition;

        public RetryDurableDefinitionBuilder(IDependencyConfigurator dependencyConfigurator)
        {
            this.dependencyConfigurator = dependencyConfigurator;
        }

        public RetryDurableDefinitionBuilder Handle<TException>()
            where TException : Exception
            => this.Handle(kafkaRetryContext => kafkaRetryContext.Exception is TException);

        public RetryDurableDefinitionBuilder Handle<TException>(Func<TException, bool> rule)
            where TException : Exception
            => this.Handle(context => context.Exception is TException ex && rule(ex));

        public RetryDurableDefinitionBuilder Handle(Func<RetryContext, bool> func)
        {
            this.retryWhenExceptions.Add(func);
            return this;
        }

        public RetryDurableDefinitionBuilder HandleAnyException()
            => this.Handle(kafkaRetryContext => true);

        public RetryDurableDefinitionBuilder WithEmbeddedRetryCluster(
            IClusterConfigurationBuilder cluster,
            Action<RetryDurableEmbeddedClusterDefinitionBuilder> configure
            )
        {
            var retryDurableEmbeddedClusterBuilder = new RetryDurableEmbeddedClusterDefinitionBuilder(cluster);
            configure(retryDurableEmbeddedClusterBuilder);
            retryDurableEmbeddedClusterBuilder.Build();

            return this;
        }

        public RetryDurableDefinitionBuilder WithQueuePollingJobConfiguration(Action<RetryDurableQueuePollingJobDefinitionBuilder> configure)
        {
            var retryDurablePollingDefinitionBuilder = new RetryDurableQueuePollingJobDefinitionBuilder();
            configure(retryDurablePollingDefinitionBuilder);
            this.retryDurablePollingDefinition = retryDurablePollingDefinitionBuilder.Build();

            return this;
        }

        public RetryDurableDefinitionBuilder WithRepositoryProvider(IRetryDurableQueueRepositoryProvider retryDurableRepositoryProvider)
        {
            this.retryDurableRepositoryProvider = retryDurableRepositoryProvider;

            return this;
        }

        public RetryDurableDefinitionBuilder WithRetryPlanBeforeRetryDurable(Action<RetryDurableRetryPlanBeforeDefinitionBuilder> configure)
        {
            var retryDurableRetryPlanBeforeDefinitionBuilder = new RetryDurableRetryPlanBeforeDefinitionBuilder();
            configure(retryDurableRetryPlanBeforeDefinitionBuilder);
            this.retryDurableRetryPlanBeforeDefinition = retryDurableRetryPlanBeforeDefinitionBuilder.Build();

            return this;
        }

        internal RetryDurableDefinition Build()
        {
            var retryDurableDefinition =
                   new RetryDurableDefinition(
                       this.retryWhenExceptions,
                       this.retryDurableRetryPlanBeforeDefinition,
                       this.retryDurablePollingDefinition
                   );

            this.dependencyConfigurator.AddSingleton<IRetryDurableDefinition>(retryDurableDefinition);
            this.dependencyConfigurator.AddSingleton<IMessageHeadersAdapter>(new MessageHeadersAdapter());
            this.dependencyConfigurator.AddSingleton<IGzipCompressor>(new GzipCompressor());
            this.dependencyConfigurator.AddSingleton<IUtf8Encoder>(new Utf8Encoder());
            this.dependencyConfigurator.AddSingleton<INewtonsoftJsonSerializer>(new NewtonsoftJsonSerializer());
            this.dependencyConfigurator
                .AddSingleton<IMessageAdapter>(
                    resolver =>
                        new MessageAdapter(
                            resolver.Resolve<IGzipCompressor>(),
                            resolver.Resolve<INewtonsoftJsonSerializer>(),
                            resolver.Resolve<IUtf8Encoder>()));

            this.dependencyConfigurator
                .AddSingleton<IRetryDurableQueueRepository>(
                    resolver =>
                        new RetryDurableQueueRepository(
                            this.retryDurableRepositoryProvider,
                            new IUpdateRetryQueueItemHandler[]
                            {
                                new UpdateRetryQueueItemStatusHandler(this.retryDurableRepositoryProvider),
                                new UpdateRetryQueueItemExecutionInfoHandler(this.retryDurableRepositoryProvider)
                            },
                            resolver.Resolve<IMessageHeadersAdapter>(),
                            resolver.Resolve<IMessageAdapter>(),
                            resolver.Resolve<IUtf8Encoder>(),
                            this.retryDurablePollingDefinition));

            this.dependencyConfigurator
                .AddSingleton<IQueueTrackerCoordinator>(
                    resolver =>
                        new QueueTrackerCoordinator(
                            new QueueTrackerFactory(
                                resolver.Resolve<IRetryDurableQueueRepository>(),
                                resolver.Resolve<ILogHandler>(),
                                resolver.Resolve<IMessageHeadersAdapter>(),
                                resolver.Resolve<IMessageAdapter>(),
                                resolver.Resolve<IUtf8Encoder>(),
                                resolver.Resolve<IProducerAccessor>().GetProducer(RetryDurableConstants.EmbeddedProducerName),
                                this.retryDurablePollingDefinition
                            ),
                            this.retryDurablePollingDefinition
                        )
                );

            return retryDurableDefinition;
        }
    }
}