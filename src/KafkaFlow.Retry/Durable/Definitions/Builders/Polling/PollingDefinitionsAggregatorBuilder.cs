using System;
using System.Collections.Generic;
using System.Linq;
using Dawn;
using KafkaFlow.Retry.Durable.Definitions.Polling;

namespace KafkaFlow.Retry;

public class PollingDefinitionsAggregatorBuilder
{
    private readonly CleanupPollingDefinitionBuilder cleanupPollingDefinitionBuilder;
    private readonly List<PollingDefinition> pollingDefinitions;
    private readonly RetryDurablePollingDefinitionBuilder retryDurablePollingDefinitionBuilder;
    private string schedulerId;

    public PollingDefinitionsAggregatorBuilder()
    {
            cleanupPollingDefinitionBuilder = new CleanupPollingDefinitionBuilder();
            retryDurablePollingDefinitionBuilder = new RetryDurablePollingDefinitionBuilder();

            pollingDefinitions = new List<PollingDefinition>();
        }

    public PollingDefinitionsAggregatorBuilder WithCleanupPollingConfiguration(Action<CleanupPollingDefinitionBuilder> configure)
    {
            Guard.Argument(configure, nameof(configure)).NotNull();

            configure(cleanupPollingDefinitionBuilder);
            var cleanupPollingDefinition = cleanupPollingDefinitionBuilder.Build();

            pollingDefinitions.Add(cleanupPollingDefinition);

            return this;
        }

    public PollingDefinitionsAggregatorBuilder WithRetryDurablePollingConfiguration(Action<RetryDurablePollingDefinitionBuilder> configure)
    {
            Guard.Argument(configure, nameof(configure)).NotNull();

            configure(retryDurablePollingDefinitionBuilder);
            var retryDurablepollingDefinition = retryDurablePollingDefinitionBuilder.Build();

            pollingDefinitions.Add(retryDurablepollingDefinition);

            return this;
        }

    public PollingDefinitionsAggregatorBuilder WithSchedulerId(string schedulerId)
    {
            this.schedulerId = schedulerId;
            return this;
        }

    internal PollingDefinitionsAggregator Build()
    {
            if (retryDurablePollingDefinitionBuilder.Required)
            {
                ValidateRequiredPollingDefinition(PollingJobType.RetryDurable);
            }

            if (cleanupPollingDefinitionBuilder.Required)
            {
                ValidateRequiredPollingDefinition(PollingJobType.Cleanup);
            }

            return new PollingDefinitionsAggregator(schedulerId, pollingDefinitions);
        }

    private void ValidateRequiredPollingDefinition(PollingJobType pollingJobType)
    {
            Guard.Argument(pollingDefinitions.Any(pd => pd.PollingJobType == pollingJobType), nameof(pollingDefinitions))
                 .True($"The polling job {pollingJobType} must be defined.");
        }
}