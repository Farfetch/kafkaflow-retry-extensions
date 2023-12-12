using System.Collections.Generic;
using System.Linq;
using Dawn;

namespace KafkaFlow.Retry.Durable.Definitions.Polling;

internal class PollingDefinitionsAggregator
{
    public PollingDefinitionsAggregator(string schedulerId, IEnumerable<PollingDefinition> pollingDefinitions)
    {
        Guard.Argument(schedulerId, nameof(schedulerId)).NotNull().NotEmpty();
        Guard.Argument(pollingDefinitions, nameof(pollingDefinitions)).NotNull().NotEmpty();

        var pollingJobTypes = pollingDefinitions.Select(pd => pd.PollingJobType);
        Guard.Argument(pollingJobTypes, nameof(pollingJobTypes)).DoesNotContainDuplicate();

        this.SchedulerId = schedulerId;
        this.PollingDefinitions = pollingDefinitions.ToDictionary(pd => pd.PollingJobType, pd => pd);
    }

    public IDictionary<PollingJobType, PollingDefinition> PollingDefinitions { get; }

    public string SchedulerId { get; }
}