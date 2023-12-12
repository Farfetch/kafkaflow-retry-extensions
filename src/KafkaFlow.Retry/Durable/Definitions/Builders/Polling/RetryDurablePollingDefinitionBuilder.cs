using KafkaFlow.Retry.Durable.Definitions.Polling;

namespace KafkaFlow.Retry;

public class RetryDurablePollingDefinitionBuilder : PollingDefinitionBuilder<RetryDurablePollingDefinitionBuilder>
{
    protected int expirationIntervalFactor = 1;
    protected int fetchSize = 256;

    internal override bool Required => true;

    public RetryDurablePollingDefinitionBuilder WithExpirationIntervalFactor(int expirationIntervalFactor)
    {
            this.expirationIntervalFactor = expirationIntervalFactor;
            return this;
        }

    public RetryDurablePollingDefinitionBuilder WithFetchSize(int fetchSize)
    {
            this.fetchSize = fetchSize;
            return this;
        }

    internal RetryDurablePollingDefinition Build()
    {
            return new RetryDurablePollingDefinition(
                this.enabled,
                this.cronExpression,
                this.fetchSize,
                this.expirationIntervalFactor
            );
        }
}