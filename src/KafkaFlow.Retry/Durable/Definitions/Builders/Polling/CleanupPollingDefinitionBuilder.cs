namespace KafkaFlow.Retry
{
    using Dawn;
    using KafkaFlow.Retry.Durable.Definitions.Polling;

    public class CleanupPollingDefinitionBuilder : PollingDefinitionBuilder<CleanupPollingDefinitionBuilder>
    {
        private int rowsPerRequest = 256;
        private int timeToLiveInDays;

        public CleanupPollingDefinitionBuilder WithRowsPerRequest(int rowsPerRequest)
        {
            this.rowsPerRequest = rowsPerRequest;
            return this;
        }

        public CleanupPollingDefinitionBuilder WithTimeToLiveInDays(int timeToLiveInDays)
        {
            this.timeToLiveInDays = timeToLiveInDays;
            return this;
        }

        internal CleanupPollingDefinition Build()
        {
            if (this.enabled)
            {
                Guard.Argument(this.timeToLiveInDays, nameof(this.timeToLiveInDays)).Positive(n => $"A positive {nameof(this.timeToLiveInDays)} must be defined.");
            }

            return new CleanupPollingDefinition(
                this.enabled,
                this.cronExpression,
                this.timeToLiveInDays,
                this.rowsPerRequest
            );
        }
    }
}