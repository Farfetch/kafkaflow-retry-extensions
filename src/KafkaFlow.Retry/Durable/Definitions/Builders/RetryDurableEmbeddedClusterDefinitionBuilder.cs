namespace KafkaFlow.Retry
{
    using System;
    using System.Linq;
    using Dawn;
    using KafkaFlow.Configuration;
    using KafkaFlow.Retry.Durable;
    using KafkaFlow.Retry.Durable.Polling;
    using KafkaFlow.Serializer;
    using KafkaFlow.TypedHandler;

    public class RetryDurableEmbeddedClusterDefinitionBuilder
    {
        private const int DefaultPartitionElection = 0;
        private readonly IClusterConfigurationBuilder cluster;
        private bool enabled;
        private Type messageType;
        private int retryConsumerBufferSize;
        private int retryConsumerWorkersCount;
        private RetryConsumerStrategy retryConusmerStrategy = RetryConsumerStrategy.GuaranteeOrderedConsumption;
        private string retryTopicName;
        private Action<TypedHandlerConfigurationBuilder> retryTypeHandlers;

        public RetryDurableEmbeddedClusterDefinitionBuilder(IClusterConfigurationBuilder cluster)
        {
            this.cluster = cluster;
        }

        public RetryDurableEmbeddedClusterDefinitionBuilder Enabled(bool enabled)
        {
            this.enabled = enabled;
            return this;
        }

        public RetryDurableEmbeddedClusterDefinitionBuilder WithRetryConsumerBufferSize(int retryConsumerBufferSize)
        {
            this.retryConsumerBufferSize = retryConsumerBufferSize;
            return this;
        }

        public RetryDurableEmbeddedClusterDefinitionBuilder WithRetryConsumerWorkersCount(int retryConsumerWorkersCount)
        {
            this.retryConsumerWorkersCount = retryConsumerWorkersCount;
            return this;
        }

        public RetryDurableEmbeddedClusterDefinitionBuilder WithRetryConusmerStrategy(RetryConsumerStrategy retryConusmerStrategy)
        {
            this.retryConusmerStrategy = retryConusmerStrategy;
            return this;
        }

        public RetryDurableEmbeddedClusterDefinitionBuilder WithRetryTopicName(string retryTopicName)
        {
            this.retryTopicName = retryTopicName;
            return this;
        }

        public RetryDurableEmbeddedClusterDefinitionBuilder WithRetryTypedHandlers(Action<TypedHandlerConfigurationBuilder> retryTypeHandlers)
        {
            this.retryTypeHandlers = retryTypeHandlers;
            return this;
        }

        internal void Build()
        {
            if (!enabled)
            {
                return;
            }

            Guard.Argument(this.cluster).NotNull("A cluster configuration builder should be passed");
            Guard.Argument(this.retryTopicName).NotNull("A retry topic name should be defined");
            Guard.Argument(this.retryTypeHandlers).NotNull("A retry type handler should be defined");
            Guard.Argument(this.retryConsumerBufferSize)
                .NotZero("A buffer size great than zero should be defined")
                .NotNegative(x => "A buffer size great than zero should be defined");
            Guard.Argument(this.retryConsumerWorkersCount)
                .NotZero("A buffer size great than zero should be defined")
                .NotNegative(x => "A buffer size great than zero should be defined");
            Guard.Argument(this.messageType).NotNull("A message type should be defined");

            this.cluster
                .AddProducer(
                    RetryDurableConstants.EmbeddedProducerName,
                    producer => producer
                        .DefaultTopic(this.retryTopicName)
                        .WithCompression(Confluent.Kafka.CompressionType.Gzip)
                        .WithAcks(Acks.All) // TODO: this settings should be reviewed
                )
                .AddConsumer(
                    consumer => consumer
                        .Topic(this.retryTopicName)
                        .WithGroupId(RetryDurableConstants.EmbeddedConsumerGroupId)
                        .WithName(RetryDurableConstants.EmbeddedConsumerName)
                        .WithBufferSize(this.retryConsumerBufferSize)
                        .WithWorkersCount(this.retryConsumerWorkersCount)
                        .WithAutoOffsetReset(AutoOffsetReset.Latest) // TODO: this settings should be reviewed
                        .WithPartitionsAssignedHandler(
                            (resolver, partitionsAssignedHandler) =>
                            {
                                if (partitionsAssignedHandler is object
                                 && partitionsAssignedHandler.Any(tp => tp.Partition == DefaultPartitionElection))
                                {
                                    resolver
                                        .Resolve<IQueueTrackerCoordinator>()
                                        .Initialize();
                                }
                            })
                        .WithPartitionsRevokedHandler(
                            (resolver, partitionsRevokedHandler) =>
                            {
                                resolver
                                    .Resolve<IQueueTrackerCoordinator>()
                                    .Shutdown();
                            })
                        .AddMiddlewares(
                            middlewares => middlewares
                                .AddSingleTypeSerializer<ProtobufNetSerializer>(this.messageType)
                                .RetryConsumerStrategy(this.retryConusmerStrategy)
                                .Add<RetryDurableConsumerValidationMiddleware>()
                                .AddTypedHandlers(this.retryTypeHandlers)
                        )
                );
        }

        internal RetryDurableEmbeddedClusterDefinitionBuilder WithMessageType(Type messageType)
        {
            this.messageType = messageType;
            return this;
        }
    }
}