namespace KafkaFlow.Retry
{
    using KafkaFlow.Retry.Durable;

    public class KafkaRetryDurablePollingDefinitionBuilder
    {
        private string cronExpression;
        private bool enabled;
        private int expirationIntervalFactor;
        private int fetchSize;

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

        internal KafkaRetryDurablePollingDefinition Build()
        {
            return new KafkaRetryDurablePollingDefinition(
                enabled,
                cronExpression,
                fetchSize,
                expirationIntervalFactor);
        }
    }
}