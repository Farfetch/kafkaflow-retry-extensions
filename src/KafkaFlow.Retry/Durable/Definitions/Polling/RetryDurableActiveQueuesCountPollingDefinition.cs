using System;
using Dawn;

namespace KafkaFlow.Retry.Durable.Definitions.Polling;
internal class RetryDurableActiveQueuesCountPollingDefinition : PollingDefinition
{
    public RetryDurableActiveQueuesCountPollingDefinition(
        bool enabled, 
        string cronExpression,
        Action<long> activeQueues) 
        : base(enabled, cronExpression)
    {
        Guard.Argument(activeQueues, nameof(activeQueues)).NotNull();
        ActiveQueues = activeQueues;
    }

    public override PollingJobType PollingJobType => PollingJobType.RetryDurableActiveQueuesCount;

    public Action<long> ActiveQueues { get; }
}
