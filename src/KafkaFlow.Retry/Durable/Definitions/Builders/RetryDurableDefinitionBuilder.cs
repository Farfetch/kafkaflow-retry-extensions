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

namespace KafkaFlow.Retry;

public class RetryDurableDefinitionBuilder
{
    private readonly List<Func<RetryContext, bool>> _retryWhenExceptions = new();
    private JsonSerializerSettings _jsonSerializerSettings = new();
    private Type _messageType;
    private PollingDefinitionsAggregator _pollingDefinitionsAggregator;
    private RetryDurableEmbeddedClusterDefinitionBuilder _retryDurableEmbeddedClusterDefinitionBuilder;
    private IRetryDurableQueueRepositoryProvider _retryDurableRepositoryProvider;
    private RetryDurableRetryPlanBeforeDefinition _retryDurableRetryPlanBeforeDefinition;

    internal RetryDurableDefinitionBuilder()
    {
    }

    public RetryDurableDefinitionBuilder Handle<TException>()
        where TException : Exception
    {
        return Handle(kafkaRetryContext => kafkaRetryContext.Exception is TException);
    }

    public RetryDurableDefinitionBuilder Handle<TException>(Func<TException, bool> rule)
        where TException : Exception
    {
        return Handle(context => context.Exception is TException ex && rule(ex));
    }

    public RetryDurableDefinitionBuilder Handle(Func<RetryContext, bool> func)
    {
        _retryWhenExceptions.Add(func);
        return this;
    }

    public RetryDurableDefinitionBuilder HandleAnyException()
    {
        return Handle(kafkaRetryContext => true);
    }

    public RetryDurableDefinitionBuilder WithEmbeddedRetryCluster(
        IClusterConfigurationBuilder cluster,
        Action<RetryDurableEmbeddedClusterDefinitionBuilder> configure
    )
    {
        Guard.Argument(configure, nameof(configure)).NotNull();

        _retryDurableEmbeddedClusterDefinitionBuilder = new RetryDurableEmbeddedClusterDefinitionBuilder(cluster);
        configure(_retryDurableEmbeddedClusterDefinitionBuilder);

        return this;
    }

    public RetryDurableDefinitionBuilder WithMessageSerializeSettings(JsonSerializerSettings jsonSerializerSettings)
    {
        _jsonSerializerSettings = jsonSerializerSettings;
        return this;
    }

    public RetryDurableDefinitionBuilder WithMessageType(Type messageType)
    {
        _messageType = messageType;
        return this;
    }

    public RetryDurableDefinitionBuilder WithPollingJobsConfiguration(
        Action<PollingDefinitionsAggregatorBuilder> configure)
    {
        Guard.Argument(configure, nameof(configure)).NotNull();

        var pollingDefinitionsAggregatorBuilder = new PollingDefinitionsAggregatorBuilder();
        configure(pollingDefinitionsAggregatorBuilder);
        _pollingDefinitionsAggregator = pollingDefinitionsAggregatorBuilder.Build();

        return this;
    }

    public RetryDurableDefinitionBuilder WithRepositoryProvider(
        IRetryDurableQueueRepositoryProvider retryDurableRepositoryProvider)
    {
        _retryDurableRepositoryProvider = retryDurableRepositoryProvider;

        return this;
    }

    public RetryDurableDefinitionBuilder WithRetryPlanBeforeRetryDurable(
        Action<RetryDurableRetryPlanBeforeDefinitionBuilder> configure)
    {
        Guard.Argument(configure, nameof(configure)).NotNull();

        var retryDurableRetryPlanBeforeDefinitionBuilder = new RetryDurableRetryPlanBeforeDefinitionBuilder();
        configure(retryDurableRetryPlanBeforeDefinitionBuilder);
        _retryDurableRetryPlanBeforeDefinition = retryDurableRetryPlanBeforeDefinitionBuilder.Build();

        return this;
    }

    internal RetryDurableDefinition Build()
    {
        // TODO: Guard the exceptions and retry plan
        Guard.Argument(_retryDurableRepositoryProvider).NotNull("A repository should be defined");
        Guard.Argument(_messageType).NotNull("A message type should be defined");

        var triggerProvider = new TriggerProvider();
        var utf8Encoder = new Utf8Encoder();
        var gzipCompressor = new GzipCompressor();
        var newtonsoftJsonSerializer = new NewtonsoftJsonSerializer(_jsonSerializerSettings);
        var messageAdapter = new NewtonsoftJsonMessageAdapter(gzipCompressor, newtonsoftJsonSerializer, utf8Encoder);
        var messageHeadersAdapter = new MessageHeadersAdapter();

        var retryDurableQueueRepository =
            new RetryDurableQueueRepository(
                _retryDurableRepositoryProvider,
                new IUpdateRetryQueueItemHandler[]
                {
                    new UpdateRetryQueueItemStatusHandler(_retryDurableRepositoryProvider),
                    new UpdateRetryQueueItemExecutionInfoHandler(_retryDurableRepositoryProvider)
                },
                messageHeadersAdapter,
                messageAdapter,
                utf8Encoder,
                _pollingDefinitionsAggregator);

        _retryDurableEmbeddedClusterDefinitionBuilder
            .Build(
                _messageType,
                retryDurableQueueRepository,
                gzipCompressor,
                utf8Encoder,
                newtonsoftJsonSerializer,
                messageHeadersAdapter,
                _pollingDefinitionsAggregator,
                triggerProvider
            );

        return new RetryDurableDefinition(
            _retryWhenExceptions,
            _retryDurableRetryPlanBeforeDefinition,
            retryDurableQueueRepository
        );
    }
}