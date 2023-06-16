namespace KafkaFlow.Retry
{
    using System;
    using System.Linq;
    using Dawn;
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
    using KafkaFlow.TypedHandler;

    public class RetryDurableEmbeddedClusterDefinitionBuilder
    {
        private const int DefaultPartitionElection = 0;
        private readonly IClusterConfigurationBuilder cluster;
        private bool enabled;
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

        public RetryDurableEmbeddedClusterDefinitionBuilder WithRetryConsumerStrategy(RetryConsumerStrategy retryConusmerStrategy)
        {
            this.retryConusmerStrategy = retryConusmerStrategy;
            return this;
        }

        public RetryDurableEmbeddedClusterDefinitionBuilder WithRetryConsumerWorkersCount(int retryConsumerWorkersCount)
        {
            this.retryConsumerWorkersCount = retryConsumerWorkersCount;
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

        internal void Build(
            Type messageType,
            IRetryDurableQueueRepository retryDurableQueueRepository,
            IGzipCompressor gzipCompressor,
            IUtf8Encoder utf8Encoder,
            INewtonsoftJsonSerializer newtonsoftJsonSerializer,
            IMessageAdapter messageAdapter,
            IMessageHeadersAdapter messageHeadersAdapter,
            RetryDurablePollingDefinition retryDurablePollingDefinition
            )
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

            var producerName = $"{RetryDurableConstants.EmbeddedProducerName}-{this.retryTopicName}";
            var consumerGroupId = $"{RetryDurableConstants.EmbeddedConsumerName}-{this.retryTopicName}";

            var queueTrackerCoordinator =
                new QueueTrackerCoordinator(
                    new QueueTrackerFactory(
                        retryDurableQueueRepository,
                        messageHeadersAdapter,
                        messageAdapter,
                        utf8Encoder
                    )
                );

            this.cluster
                .AddProducer(
                    producerName,
                    producer => producer
                        .DefaultTopic(this.retryTopicName)
                        .WithCompression(Confluent.Kafka.CompressionType.Gzip)
                        .WithAcks(Acks.Leader)
                )
                .AddConsumer(
                    consumer => consumer
                        .Topic(this.retryTopicName)
                        .WithGroupId(consumerGroupId)
                        .WithBufferSize(this.retryConsumerBufferSize)
                        .WithWorkersCount(this.retryConsumerWorkersCount)
                        .WithAutoOffsetReset(AutoOffsetReset.Earliest)
                        .WithPartitionsAssignedHandler(
                            (resolver, partitionsAssignedHandler) =>
                            {
                                var log = resolver.Resolve<ILogHandler>();
                                log.Info(
                                    "Partitions Assigned",
                                    new
                                    {
                                        PartitionsAssigned = partitionsAssignedHandler
                                    });

                                if (partitionsAssignedHandler is object
                                 && partitionsAssignedHandler.Any(tp => tp.Partition == DefaultPartitionElection))
                                {
                                    log.Info(
                                        "Default Partition Assigned",
                                        new
                                        {
                                            DefaultPartitionElection
                                        });

                                    queueTrackerCoordinator
                                        .ScheduleJob(
                                            retryDurablePollingDefinition,
                                            resolver.Resolve<IProducerAccessor>().GetProducer(producerName),
                                            log);
                                }
                            })
                        .WithPartitionsRevokedHandler(
                            (resolver, partitionsRevokedHandler) =>
                            {
                                var log = resolver.Resolve<ILogHandler>();
                                log.Info(
                                    "Partitions Revoked",
                                    new
                                    {
                                        PartitionsRevoked = partitionsRevokedHandler
                                    });

                                queueTrackerCoordinator.UnscheduleJob();
                            })
                        .AddMiddlewares(
                            middlewares => middlewares
                                .Add(resolver => new RetryDurableConsumerCompressorMiddleware(gzipCompressor))
                                .Add(resolver => new RetryDurableConsumerUtf8EncoderMiddleware(utf8Encoder))
                                .Add(resolver => new RetryDurableConsumerNewtonsoftJsonSerializerMiddleware(newtonsoftJsonSerializer, messageType))
                                .WithRetryConsumerStrategy(this.retryConusmerStrategy, retryDurableQueueRepository, utf8Encoder)
                                .Add(resolver =>
                                    new RetryDurableConsumerValidationMiddleware(
                                            resolver.Resolve<ILogHandler>(),
                                            retryDurableQueueRepository,
                                            utf8Encoder
                                        ))
                                .AddTypedHandlers(this.retryTypeHandlers)
                        )
                );
        }
    }
}