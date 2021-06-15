namespace KafkaFlow.Retry
{
    using KafkaFlow.Retry.Durable.Definitions;

    public class RetryDurableQueuePollingJobDefinitionBuilder
    {
        private string cronExpression;
        private bool enabled;
        private int expirationIntervalFactor = 1;
        private int fetchSize = 96;
        private string id = string.Empty;

        public RetryDurableQueuePollingJobDefinitionBuilder Enabled(bool enabled)
        {
            this.enabled = enabled;
            return this;
        }

        public RetryDurableQueuePollingJobDefinitionBuilder WithCronExpression(string cronExpression)
        {
            this.cronExpression = cronExpression;
            return this;
        }

        public RetryDurableQueuePollingJobDefinitionBuilder WithExpirationIntervalFactor(int expirationIntervalFactor)
        {
            this.expirationIntervalFactor = expirationIntervalFactor;
            return this;
        }

        public RetryDurableQueuePollingJobDefinitionBuilder WithFetchSize(int fetchSize)
        {
            this.fetchSize = fetchSize;
            return this;
        }

        public RetryDurableQueuePollingJobDefinitionBuilder WithId(string id)
        {
            this.id = id;
            return this;
        }

        internal RetryDurablePollingDefinition Build()
        {
            return new RetryDurablePollingDefinition(
                this.enabled,
                this.cronExpression,
                this.fetchSize,
                this.expirationIntervalFactor,
                this.id
            );
        }
    }
}