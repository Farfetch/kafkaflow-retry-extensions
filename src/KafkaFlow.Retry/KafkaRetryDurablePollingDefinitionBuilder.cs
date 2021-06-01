namespace KafkaFlow.Retry
{
    using System;
    using KafkaFlow.Retry.Durable;
    using KafkaFlow.Retry.Durable.Polling.Strategies;

    public class KafkaRetryDurablePollingDefinitionBuilder
    {
        private string cronExpression;
        private bool enabled;
        private int expirationIntervalFactor = 1;
        private int fetchSize = 96;
        private PollingJobStrategyType pollingJobStrategy = PollingJobStrategyType.Earliest;

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

        public KafkaRetryDurablePollingDefinitionBuilder WithPollingStrategy(string pollingStrategy)
        {
            this.pollingJobStrategy = (PollingJobStrategyType)Enum.Parse(typeof(PollingJobStrategyType), pollingStrategy, true);
            return this;
        }

        internal KafkaRetryDurablePollingDefinition Build()
        {
            return new KafkaRetryDurablePollingDefinition(
                this.enabled,
                this.cronExpression,
                this.fetchSize,
                this.expirationIntervalFactor,
                this.pollingJobStrategy
            );
        }
    }
}