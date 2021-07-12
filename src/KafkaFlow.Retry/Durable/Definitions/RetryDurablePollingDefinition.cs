namespace KafkaFlow.Retry.Durable.Definitions
{
    using Dawn;

    internal class RetryDurablePollingDefinition : IRetryDurablePollingDefinition
    {
        public RetryDurablePollingDefinition(
            bool enabled,
            string cronExpression,
            int fetchSize,
            int expirationIntervalFactor,
            string id)
        {
            if (enabled)
            {
                Guard.Argument(Quartz.CronExpression.IsValidExpression(cronExpression), nameof(cronExpression))
                     .True("The cron expression that was defined is not valid");
            }

            Guard.Argument(id, nameof(id)).NotNull().NotEmpty();
            Guard.Argument(fetchSize, nameof(fetchSize)).Positive();
            Guard.Argument(expirationIntervalFactor, nameof(expirationIntervalFactor)).Positive();

            this.CronExpression = cronExpression;
            this.Enabled = enabled;
            this.FetchSize = fetchSize;
            this.ExpirationIntervalFactor = expirationIntervalFactor;
            this.Id = id;
        }

        public string CronExpression { get; }

        public bool Enabled { get; }

        public int ExpirationIntervalFactor { get; }

        public int FetchSize { get; }

        public string Id { get; }
    }
}