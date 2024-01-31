using System;
using System.Collections.Generic;
using System.Linq;
using Dawn;
using KafkaFlow.Retry.Durable.Definitions.Polling;

namespace KafkaFlow.Retry;

public class PollingDefinitionsAggregatorBuilder
{
    private readonly CleanupPollingDefinitionBuilder _cleanupPollingDefinitionBuilder;
    private readonly List<PollingDefinition> _pollingDefinitions;
    private readonly RetryDurablePollingDefinitionBuilder _retryDurablePollingDefinitionBuilder;
    private string _schedulerId;

    public PollingDefinitionsAggregatorBuilder()
    {
        _cleanupPollingDefinitionBuilder = new CleanupPollingDefinitionBuilder();
        _retryDurablePollingDefinitionBuilder = new RetryDurablePollingDefinitionBuilder();

        _pollingDefinitions = new List<PollingDefinition>();
    }

    public PollingDefinitionsAggregatorBuilder WithCleanupPollingConfiguration(
        Action<CleanupPollingDefinitionBuilder> configure)
    {
        Guard.Argument(configure, nameof(configure)).NotNull();

        configure(_cleanupPollingDefinitionBuilder);
        var cleanupPollingDefinition = _cleanupPollingDefinitionBuilder.Build();

        _pollingDefinitions.Add(cleanupPollingDefinition);

        return this;
    }

    public PollingDefinitionsAggregatorBuilder WithRetryDurablePollingConfiguration(
        Action<RetryDurablePollingDefinitionBuilder> configure)
    {
        Guard.Argument(configure, nameof(configure)).NotNull();

        configure(_retryDurablePollingDefinitionBuilder);
        var retryDurablepollingDefinition = _retryDurablePollingDefinitionBuilder.Build();

        _pollingDefinitions.Add(retryDurablepollingDefinition);

        return this;
    }

    public PollingDefinitionsAggregatorBuilder WithSchedulerId(string schedulerId)
    {
        _schedulerId = schedulerId;
        return this;
    }

    internal PollingDefinitionsAggregator Build()
    {
        if (_retryDurablePollingDefinitionBuilder.Required)
        {
            ValidateRequiredPollingDefinition(PollingJobType.RetryDurable);
        }

        if (_cleanupPollingDefinitionBuilder.Required)
        {
            ValidateRequiredPollingDefinition(PollingJobType.Cleanup);
        }

        return new PollingDefinitionsAggregator(_schedulerId, _pollingDefinitions);
    }

    private void ValidateRequiredPollingDefinition(PollingJobType pollingJobType)
    {
        Guard.Argument(_pollingDefinitions.Any(pd => pd.PollingJobType == pollingJobType), nameof(_pollingDefinitions))
            .True($"The polling job {pollingJobType} must be defined.");
    }
}