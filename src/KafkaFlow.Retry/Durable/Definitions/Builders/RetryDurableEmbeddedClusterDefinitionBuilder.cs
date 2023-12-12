using System;
using System.Linq;
using Confluent.Kafka;
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
    private readonly IClusterConfigurationBuilder _cluster;
    private bool _enabled;
    private int _retryConsumerBufferSize;
    private int _retryConsumerWorkersCount;
    private RetryConsumerStrategy _retryConusmerStrategy = RetryConsumerStrategy.GuaranteeOrderedConsumption;
    private string _retryTopicName;
    private Action<TypedHandlerConfigurationBuilder> _retryTypeHandlers;

    public RetryDurableEmbeddedClusterDefinitionBuilder(IClusterConfigurationBuilder cluster)
    {
        _cluster = cluster;
    }

    public RetryDurableEmbeddedClusterDefinitionBuilder Enabled(bool enabled)
    {
        _enabled = enabled;
        return this;
    }

    public RetryDurableEmbeddedClusterDefinitionBuilder WithRetryConsumerBufferSize(int retryConsumerBufferSize)
    {
        _retryConsumerBufferSize = retryConsumerBufferSize;
        return this;
    }

    public RetryDurableEmbeddedClusterDefinitionBuilder WithRetryConsumerStrategy(
        RetryConsumerStrategy retryConusmerStrategy)
    {
        _retryConusmerStrategy = retryConusmerStrategy;
        return this;
    }

    public RetryDurableEmbeddedClusterDefinitionBuilder WithRetryConsumerWorkersCount(int retryConsumerWorkersCount)
    {
        _retryConsumerWorkersCount = retryConsumerWorkersCount;
        return this;
    }

    public RetryDurableEmbeddedClusterDefinitionBuilder WithRetryTopicName(string retryTopicName)
    {
        _retryTopicName = retryTopicName;
        return this;
    }

    public RetryDurableEmbeddedClusterDefinitionBuilder WithRetryTypedHandlers(
        Action<TypedHandlerConfigurationBuilder> retryTypeHandlers)
    {
        _retryTypeHandlers = retryTypeHandlers;
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
        if (!_enabled)
        {
            return;
        }

        Guard.Argument(_cluster).NotNull("A cluster configuration builder should be passed");
        Guard.Argument(_retryTopicName).NotNull("A retry topic name should be defined");
        Guard.Argument(_retryTypeHandlers).NotNull("A retry type handler should be defined");
        Guard.Argument(_retryConsumerBufferSize)
            .NotZero("A buffer size great than zero should be defined")
            .NotNegative(x => "A buffer size great than zero should be defined");
        Guard.Argument(_retryConsumerWorkersCount)
            .NotZero("A buffer size great than zero should be defined")
            .NotNegative(x => "A buffer size great than zero should be defined");

        var producerName = $"{RetryDurableConstants.EmbeddedProducerName}-{_retryTopicName}";
        var consumerGroupId = $"{RetryDurableConstants.EmbeddedConsumerName}-{_retryTopicName}";

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

        _cluster
            .AddProducer(
                producerName,
                producer => producer
                    .DefaultTopic(_retryTopicName)
                    .WithCompression(CompressionType.Gzip)
                    .WithAcks(Acks.Leader)
            )
            .AddConsumer(
                consumer => consumer
                    .Topic(_retryTopicName)
                    .WithGroupId(consumerGroupId)
                    .WithBufferSize(_retryConsumerBufferSize)
                    .WithWorkersCount(_retryConsumerWorkersCount)
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
                            .Add(resolver =>
                                new RetryDurableConsumerNewtonsoftJsonSerializerMiddleware(newtonsoftJsonSerializer,
                                    messageType))
                            .WithRetryConsumerStrategy(_retryConusmerStrategy, retryDurableQueueRepository, utf8Encoder)
                            .Add(resolver =>
                                new RetryDurableConsumerValidationMiddleware(
                                    resolver.Resolve<ILogHandler>(),
                                    retryDurableQueueRepository,
                                    utf8Encoder
                                ))
                            .AddTypedHandlers(_retryTypeHandlers)
                    )
            );
    }
}