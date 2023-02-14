namespace KafkaFlow.Retry.Durable.Definitions.Polling
{
    using Dawn;

    internal class CleanupPollingDefinition : PollingDefinition
    {
        public CleanupPollingDefinition(
            bool enabled,
            string cronExpression,
            int timeToLiveInDays,
            int rowsPerRequest)
            : base(PollingJobType.Cleanup, enabled, cronExpression)
        {
            if (enabled)
            {
                Guard.Argument(timeToLiveInDays, nameof(timeToLiveInDays)).Positive();
                Guard.Argument(rowsPerRequest, nameof(rowsPerRequest)).Positive();
            }

            this.TimeToLiveInDays = timeToLiveInDays;
            this.RowsPerRequest = rowsPerRequest;
        }

        public int RowsPerRequest { get; }

        public int TimeToLiveInDays { get; }
    }
}