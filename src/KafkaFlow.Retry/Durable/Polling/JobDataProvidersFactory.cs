using System.Collections.Generic;
using Dawn;
using KafkaFlow.Retry.Durable.Definitions.Polling;
using KafkaFlow.Retry.Durable.Encoders;
using KafkaFlow.Retry.Durable.Polling.Jobs;
using KafkaFlow.Retry.Durable.Repository;
using KafkaFlow.Retry.Durable.Repository.Adapters;
using Quartz;

namespace KafkaFlow.Retry.Durable.Polling;

internal class JobDataProvidersFactory : IJobDataProvidersFactory
{
    private readonly IMessageHeadersAdapter _messageHeadersAdapter;
    private readonly PollingDefinitionsAggregator _pollingDefinitionsAggregator;
    private readonly IRetryDurableQueueRepository _retryDurableQueueRepository;
    private readonly ITriggerProvider _triggerProvider;
    private readonly IUtf8Encoder _utf8Encoder;

    public JobDataProvidersFactory(
        PollingDefinitionsAggregator pollingDefinitionsAggregator,
        ITriggerProvider triggerProvider,
        IRetryDurableQueueRepository retryDurableQueueRepository,
        IMessageHeadersAdapter messageHeadersAdapter,
        IUtf8Encoder utf8Encoder)
    {
        Guard.Argument(pollingDefinitionsAggregator, nameof(pollingDefinitionsAggregator)).NotNull();
        Guard.Argument(triggerProvider, nameof(triggerProvider)).NotNull();
        Guard.Argument(retryDurableQueueRepository).NotNull();
        Guard.Argument(messageHeadersAdapter).NotNull();
        Guard.Argument(utf8Encoder).NotNull();

        _pollingDefinitionsAggregator = pollingDefinitionsAggregator;
        _retryDurableQueueRepository = retryDurableQueueRepository;
        _messageHeadersAdapter = messageHeadersAdapter;
        _utf8Encoder = utf8Encoder;
        _triggerProvider = triggerProvider;
    }

    public IEnumerable<IJobDataProvider> Create(IMessageProducer retryDurableMessageProducer, ILogHandler logHandler)
    {
        var jobDataProviders = new List<IJobDataProvider>(2);

        if (TryGetPollingDefinition<RetryDurablePollingDefinition>(PollingJobType.RetryDurable,
                out var retryDurablePollingDefinition))
        {
            jobDataProviders.Add(
                new RetryDurableJobDataProvider(
                    retryDurablePollingDefinition,
                    GetTrigger(retryDurablePollingDefinition),
                    _pollingDefinitionsAggregator.SchedulerId,
                    _retryDurableQueueRepository,
                    logHandler,
                    _messageHeadersAdapter,
                    _utf8Encoder,
                    retryDurableMessageProducer
                )
            );
        }

        if (TryGetPollingDefinition<CleanupPollingDefinition>(PollingJobType.Cleanup, out var cleanupPollingDefinition))
        {
            jobDataProviders.Add(
                new CleanupJobDataProvider(
                    cleanupPollingDefinition,
                    GetTrigger(cleanupPollingDefinition),
                    _pollingDefinitionsAggregator.SchedulerId,
                    _retryDurableQueueRepository,
                    logHandler
                )
            );
        }

        return jobDataProviders;
    }

    private ITrigger GetTrigger(PollingDefinition pollingDefinition)
    {
        return _triggerProvider.GetPollingTrigger(_pollingDefinitionsAggregator.SchedulerId, pollingDefinition);
    }

    private bool TryGetPollingDefinition<TPollingDefinition>(PollingJobType pollingJobType,
        out TPollingDefinition pollingDefinition) where TPollingDefinition : PollingDefinition
    {
        pollingDefinition = default;

        var pollingDefinitions = _pollingDefinitionsAggregator.PollingDefinitions;

        var pollingDefinitionFound = pollingDefinitions.TryGetValue(pollingJobType, out var pollingDefinitionResult);

        if (pollingDefinitionFound)
        {
            Guard.Argument(pollingDefinitionResult, nameof(pollingDefinitionResult)).NotNull()
                .Compatible<TPollingDefinition>();
            pollingDefinition = pollingDefinitionResult as TPollingDefinition;
        }

        return pollingDefinitionFound;
    }
}