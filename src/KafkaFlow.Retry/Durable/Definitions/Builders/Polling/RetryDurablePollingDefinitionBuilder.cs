using KafkaFlow.Retry.Durable.Definitions.Polling;

namespace KafkaFlow.Retry;

public class RetryDurablePollingDefinitionBuilder : PollingDefinitionBuilder<RetryDurablePollingDefinitionBuilder>
{
    protected int ExpirationIntervalFactor = 1;
    protected int FetchSize = 256;

    internal override bool Required => true;

    public RetryDurablePollingDefinitionBuilder WithExpirationIntervalFactor(int expirationIntervalFactor)
    {
        ExpirationIntervalFactor = expirationIntervalFactor;
        return this;
    }

    public RetryDurablePollingDefinitionBuilder WithFetchSize(int fetchSize)
    {
        FetchSize = fetchSize;
        return this;
    }

    internal RetryDurablePollingDefinition Build()
    {
        return new RetryDurablePollingDefinition(
            IsEnabled,
            CronExpression,
            FetchSize,
            ExpirationIntervalFactor
        );
    }
}