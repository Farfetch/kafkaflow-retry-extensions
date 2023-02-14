namespace KafkaFlow.Retry.Durable.Definitions.Polling
{
    using Dawn;

    internal class RetryDurablePollingDefinition : PollingDefinition
    {
        public RetryDurablePollingDefinition(
            bool enabled,
            string cronExpression,
            int fetchSize,
            int expirationIntervalFactor)
            : base(PollingJobType.RetryDurable, enabled, cronExpression)
        {
            Guard.Argument(fetchSize, nameof(fetchSize)).Positive();
            Guard.Argument(expirationIntervalFactor, nameof(expirationIntervalFactor)).Positive();

            this.FetchSize = fetchSize;
            this.ExpirationIntervalFactor = expirationIntervalFactor;
        }

        public int ExpirationIntervalFactor { get; }

        public int FetchSize { get; }
    }
}