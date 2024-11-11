using System;
using Dawn;
using KafkaFlow.Retry.Durable.Definitions.Polling;

namespace KafkaFlow.Retry.Durable.Definitions.Builders.Polling;

public class RetryDurableActiveQueuesCountPollingDefinitionBuilder
    : PollingDefinitionBuilder<RetryDurableActiveQueuesCountPollingDefinitionBuilder>
{
    protected Action<long> ActionToPerform;

    internal override bool Required => false;

    public RetryDurableActiveQueuesCountPollingDefinitionBuilder Do(Action<long> actionToPerform)
    {
        Guard.Argument(actionToPerform, nameof(actionToPerform)).NotNull();

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