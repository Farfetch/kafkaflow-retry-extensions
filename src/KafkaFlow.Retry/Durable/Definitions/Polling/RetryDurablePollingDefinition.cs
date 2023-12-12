using Dawn;

namespace KafkaFlow.Retry.Durable.Definitions.Polling;

internal class RetryDurablePollingDefinition : PollingDefinition
{
    public RetryDurablePollingDefinition(
        bool enabled,
        string cronExpression,
        int fetchSize,
        int expirationIntervalFactor)
        : base(enabled, cronExpression)
    {
            Guard.Argument(fetchSize, nameof(fetchSize)).Positive();
            Guard.Argument(expirationIntervalFactor, nameof(expirationIntervalFactor)).Positive();

            this.FetchSize = fetchSize;
            this.ExpirationIntervalFactor = expirationIntervalFactor;
        }

    public int ExpirationIntervalFactor { get; }

    public int FetchSize { get; }

    public override PollingJobType PollingJobType => PollingJobType.RetryDurable;
}