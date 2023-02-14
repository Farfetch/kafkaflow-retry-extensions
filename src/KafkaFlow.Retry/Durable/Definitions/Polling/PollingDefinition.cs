namespace KafkaFlow.Retry.Durable.Definitions.Polling
{
    using Dawn;

    internal abstract class PollingDefinition
    {
        protected PollingDefinition(PollingJobType pollingJobType, bool enabled, string cronExpression)
        {
            Guard.Argument(pollingJobType, nameof(pollingJobType)).NotDefault();

            if (enabled)
            {
                Guard.Argument(Quartz.CronExpression.IsValidExpression(cronExpression), nameof(cronExpression))
                     .True("The cron expression that was defined is not valid");
            }

            this.PollingJobType = pollingJobType;
            this.Enabled = enabled;
            this.CronExpression = cronExpression;
        }

        public string CronExpression { get; }

        public bool Enabled { get; }

        public PollingJobType PollingJobType { get; }
    }
}