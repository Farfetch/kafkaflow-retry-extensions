namespace KafkaFlow.Retry
{
    using KafkaFlow.Retry.Durable;

    public class KafkaRetryDurablePollingDefinitionBuilder
    {
        private string cronExpression;
        private bool enabled;
        private int expirationIntervalFactor = 1;
        private int fetchSize = 96;
        private string Id = string.Empty;
        private PollingStrategy Strategy = PollingStrategy.KeepConsumptionOrder;

        public KafkaRetryDurablePollingDefinitionBuilder Enabled(bool enabled)
        {
            this.enabled = enabled;
            return this;
        }

        public KafkaRetryDurablePollingDefinitionBuilder WithCronExpression(string cronExpression)
        {
            this.cronExpression = cronExpression;
            return this;
        }

        public KafkaRetryDurablePollingDefinitionBuilder WithExpirationIntervalFactor(int expirationIntervalFactor)
        {
            this.expirationIntervalFactor = expirationIntervalFactor;
            return this;
        }

        public KafkaRetryDurablePollingDefinitionBuilder WithFetchSize(int fetchSize)
        {
            this.fetchSize = fetchSize;
            return this;
        }

        public KafkaRetryDurablePollingDefinitionBuilder WithId(string id)
        {
            this.Id = id;
            return this;
        }

        public KafkaRetryDurablePollingDefinitionBuilder WithStrategy(PollingStrategy strategy)
        {
            this.Strategy = strategy;
            return this;
        }

        internal KafkaRetryDurablePollingDefinition Build()
        {
            return new KafkaRetryDurablePollingDefinition(
                this.enabled,
                this.cronExpression,
                this.fetchSize,
                this.expirationIntervalFactor,
                this.Strategy,
                this.Id
            );
        }
    }
}