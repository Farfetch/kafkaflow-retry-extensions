using System;
using System.Linq;
using Dawn;
using KafkaFlow.Configuration;
using KafkaFlow.Producers;
using KafkaFlow.Retry.Durable;
using KafkaFlow.Retry.Durable.Compression;
using KafkaFlow.Retry.Durable.Definitions.Polling;
using KafkaFlow.Retry.Durable.Encoders;
using KafkaFlow.Retry.Durable.Polling;
using KafkaFlow.Retry.Durable.Repository;
using KafkaFlow.Retry.Durable.Repository.Adapters;
using KafkaFlow.Retry.Durable.Serializers;

namespace KafkaFlow.Retry;

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
        IMessageHeadersAdapter messageHeadersAdapter,
        PollingDefinitionsAggregator pollingDefinitionsAggregator,
        ITriggerProvider triggerProvider)
    {
            if (!enabled)
            {
                return;
            }

            Guard.Argument(cluster).NotNull("A cluster configuration builder should be passed");
            Guard.Argument(retryTopicName).NotNull("A retry topic name should be defined");
            Guard.Argument(retryTypeHandlers).NotNull("A retry type handler should be defined");
            Guard.Argument(retryConsumerBufferSize)
                .NotZero("A buffer size great than zero should be defined")
                .NotNegative(x => "A buffer size great than zero should be defined");
            Guard.Argument(retryConsumerWorkersCount)
                .NotZero("A buffer size great than zero should be defined")
                .NotNegative(x => "A buffer size great than zero should be defined");

            var producerName = $"{RetryDurableConstants.EmbeddedProducerName}-{retryTopicName}";
            var consumerGroupId = $"{RetryDurableConstants.EmbeddedConsumerName}-{retryTopicName}";

            var queueTrackerCoordinator =
                new QueueTrackerCoordinator(
                    new QueueTrackerFactory(
                        pollingDefinitionsAggregator.SchedulerId,
                        new JobDataProvidersFactory(
                            pollingDefinitionsAggregator,
                            triggerProvider,
                            retryDurableQueueRepository,
                            messageHeadersAdapter,
                            utf8Encoder
                        )
                    )
                );

            cluster
                .AddProducer(
                    producerName,
                    producer => producer
                        .DefaultTopic(retryTopicName)
                        .WithCompression(Confluent.Kafka.CompressionType.Gzip)
                        .WithAcks(Acks.Leader)
                )
                .AddConsumer(
                    consumer => consumer
                        .Topic(retryTopicName)
                        .WithGroupId(consumerGroupId)
                        .WithBufferSize(retryConsumerBufferSize)
                        .WithWorkersCount(retryConsumerWorkersCount)
                        .WithAutoOffsetReset(AutoOffsetReset.Earliest)
                        .WithPartitionsAssignedHandler(
                            (resolver, partitionsAssignedHandler) =>
                            {
                                if (partitionsAssignedHandler is object
                                 && partitionsAssignedHandler.Any(tp => tp.Partition == DefaultPartitionElection))
                                {
                                    var log = resolver.Resolve<ILogHandler>();
                                    log.Info(
                                        "Default partition assigned",
                                        new
                                        {
                                            DefaultPartitionElection
                                        });

                                    queueTrackerCoordinator
                                        .ScheduleJobsAsync(
                                            resolver.Resolve<IProducerAccessor>().GetProducer(producerName),
                                            log)
                                        .GetAwaiter()
                                        .GetResult();
                                }
                            })
                        .WithPartitionsRevokedHandler(
                            (resolver, partitionsRevokedHandler) =>
                            {
                                queueTrackerCoordinator.UnscheduleJobsAsync().GetAwaiter().GetResult();
                            })
                        .AddMiddlewares(
                            middlewares => middlewares
                                .Add(resolver => new RetryDurableConsumerCompressorMiddleware(gzipCompressor))
                                .Add(resolver => new RetryDurableConsumerUtf8EncoderMiddleware(utf8Encoder))
                                .Add(resolver => new RetryDurableConsumerNewtonsoftJsonSerializerMiddleware(newtonsoftJsonSerializer, messageType))
                                .WithRetryConsumerStrategy(retryConusmerStrategy, retryDurableQueueRepository, utf8Encoder)
                                .Add(resolver =>
                                    new RetryDurableConsumerValidationMiddleware(
                                            resolver.Resolve<ILogHandler>(),
                                            retryDurableQueueRepository,
                                            utf8Encoder
                                        ))
                                .AddTypedHandlers(retryTypeHandlers)
                        )
                );
        }
}