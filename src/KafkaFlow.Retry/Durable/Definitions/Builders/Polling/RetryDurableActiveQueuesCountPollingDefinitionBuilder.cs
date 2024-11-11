using System;
using KafkaFlow.Retry.Durable.Definitions.Polling;

namespace KafkaFlow.Retry.Durable.Definitions.Builders.Polling;
public class RetryDurableActiveQueuesCountPollingDefinitionBuilder : PollingDefinitionBuilder<RetryDurableActiveQueuesCountPollingDefinitionBuilder>
{
    protected Action<long> ActionToPerform = null;

    internal override bool Required => false;

    public RetryDurableActiveQueuesCountPollingDefinitionBuilder Do(Action<long> actionToPerform)
    {
        ActionToPerform = actionToPerform;
        return this;
    }

    internal RetryDurableActiveQueuesCountPollingDefinition Build()
    {
        return new RetryDurableActiveQueuesCountPollingDefinition(
            IsEnabled,
            CronExpression,
            ActionToPerform
        );
    }
}
