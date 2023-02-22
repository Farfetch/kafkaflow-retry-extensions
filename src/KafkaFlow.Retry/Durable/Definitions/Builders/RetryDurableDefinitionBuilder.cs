namespace KafkaFlow.Retry
{
    using System;
    using System.Collections.Generic;
    using Dawn;
    using KafkaFlow.Configuration;
    using KafkaFlow.Retry.Durable.Compression;
    using KafkaFlow.Retry.Durable.Definitions;
    using KafkaFlow.Retry.Durable.Definitions.Polling;
    using KafkaFlow.Retry.Durable.Encoders;
    using KafkaFlow.Retry.Durable.Polling;
    using KafkaFlow.Retry.Durable.Repository;
    using KafkaFlow.Retry.Durable.Repository.Adapters;
    using KafkaFlow.Retry.Durable.Serializers;
    using Newtonsoft.Json;

    public class RetryDurableDefinitionBuilder
    {
        private readonly List<Func<RetryContext, bool>> retryWhenExceptions = new List<Func<RetryContext, bool>>();
        private JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
        private Type messageType;
        private PollingDefinitionsAggregator pollingDefinitionsAggregator;
        private RetryDurableEmbeddedClusterDefinitionBuilder retryDurableEmbeddedClusterDefinitionBuilder;
        private IRetryDurableQueueRepositoryProvider retryDurableRepositoryProvider;
        private RetryDurableRetryPlanBeforeDefinition retryDurableRetryPlanBeforeDefinition;

        internal RetryDurableDefinitionBuilder()
        { }

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
            this.retryDurableEmbeddedClusterDefinitionBuilder = new RetryDurableEmbeddedClusterDefinitionBuilder(cluster);
            configure(this.retryDurableEmbeddedClusterDefinitionBuilder);

            return this;
        }

        public RetryDurableDefinitionBuilder WithMessageSerializeSettings(JsonSerializerSettings jsonSerializerSettings)
        {
            this.jsonSerializerSettings = jsonSerializerSettings;
            return this;
        }

        public RetryDurableDefinitionBuilder WithMessageType(Type messageType)
        {
            this.messageType = messageType;
            return this;
        }

        public RetryDurableDefinitionBuilder WithPollingJobsConfiguration(Action<PollingDefinitionsAggregatorBuilder> configure)
        {
            var pollingDefinitionsAggregatorBuilder = new PollingDefinitionsAggregatorBuilder();
            configure(pollingDefinitionsAggregatorBuilder);
            this.pollingDefinitionsAggregator = pollingDefinitionsAggregatorBuilder.Build();

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
            // TODO: Guard the exceptions and retry plan
            Guard.Argument(this.retryDurableRepositoryProvider).NotNull("A repository should be defined");
            Guard.Argument(this.messageType).NotNull("A message type should be defined");

            var triggerProvider = new TriggerProvider();
            var utf8Encoder = new Utf8Encoder();
            var gzipCompressor = new GzipCompressor();
            var newtonsoftJsonSerializer = new NewtonsoftJsonSerializer(this.jsonSerializerSettings);
            var messageAdapter = new NewtonsoftJsonMessageAdapter(gzipCompressor, newtonsoftJsonSerializer, utf8Encoder);
            var messageHeadersAdapter = new MessageHeadersAdapter();

            var retryDurableQueueRepository =
                new RetryDurableQueueRepository(
                    this.retryDurableRepositoryProvider,
                    new IUpdateRetryQueueItemHandler[]
                    {
                        new UpdateRetryQueueItemStatusHandler(this.retryDurableRepositoryProvider),
                        new UpdateRetryQueueItemExecutionInfoHandler(this.retryDurableRepositoryProvider)
                    },
                    messageHeadersAdapter,
                    messageAdapter,
                    utf8Encoder,
                    this.pollingDefinitionsAggregator);

            this.retryDurableEmbeddedClusterDefinitionBuilder
                .Build(
                    this.messageType,
                    retryDurableQueueRepository,
                    gzipCompressor,
                    utf8Encoder,
                    newtonsoftJsonSerializer,
                    messageAdapter,
                    messageHeadersAdapter,
                    this.pollingDefinitionsAggregator,
                    triggerProvider
                );

            return new RetryDurableDefinition(
                this.retryWhenExceptions,
                this.retryDurableRetryPlanBeforeDefinition,
                retryDurableQueueRepository
            );
        }
    }
}