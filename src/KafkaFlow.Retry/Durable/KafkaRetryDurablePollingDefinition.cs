namespace KafkaFlow.Retry.Durable
{
    using Dawn;

    internal class KafkaRetryDurablePollingDefinition
    {
        public KafkaRetryDurablePollingDefinition(
            bool enabled,
            string cronExpression = null,
            int fetchSize = 96,
            int expirationIntervalFactor = 1)
        {
            if (enabled)
            {
                Guard
                    .Argument(Quartz.CronExpression.IsValidExpression(cronExpression), nameof(cronExpression))
                    .True("A valid cron expression is required when the polling is enabled.");
            }

            Guard.Argument(fetchSize, nameof(fetchSize)).Positive();
            Guard.Argument(expirationIntervalFactor, nameof(expirationIntervalFactor)).Positive();

            this.CronExpression = cronExpression;
            this.Enabled = enabled;
            this.FetchSize = fetchSize;
            this.ExpirationIntervalFactor = expirationIntervalFactor;
        }

        public string CronExpression { get; }

        public bool Enabled { get; }

        public int ExpirationIntervalFactor { get; }

        public int FetchSize { get; }
    }
}