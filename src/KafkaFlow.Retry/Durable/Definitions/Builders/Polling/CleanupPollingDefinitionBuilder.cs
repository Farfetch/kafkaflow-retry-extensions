namespace KafkaFlow.Retry
{
    using KafkaFlow.Retry.Durable.Definitions.Polling;

    public class CleanupPollingDefinitionBuilder : PollingDefinitionBuilder<CleanupPollingDefinitionBuilder>
    {
        private int rowsPerRequest = 256;
        private int timeToLiveInDays = 30;

        internal override bool Required => false;

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
            return new CleanupPollingDefinition(
                this.enabled,
                this.cronExpression,
                this.timeToLiveInDays,
                this.rowsPerRequest
            );
        }
    }
}