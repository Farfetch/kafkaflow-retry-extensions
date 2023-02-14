namespace KafkaFlow.Retry
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using KafkaFlow.Retry.Durable.Definitions.Polling;

    public class PollingDefinitionsAggregatorBuilder
    {
        private readonly List<PollingDefinition> pollingDefinitions;
        private string schedulerId;

        public PollingDefinitionsAggregatorBuilder()
        {
            this.pollingDefinitions = new List<PollingDefinition>();
        }

        public PollingDefinitionsAggregatorBuilder WithCleanupPollingConfiguration(Action<CleanupPollingDefinitionBuilder> configure)
        {
            var cleanupPollingDefinitionBuilder = new CleanupPollingDefinitionBuilder();
            configure(cleanupPollingDefinitionBuilder);
            var cleanupPollingDefinition = cleanupPollingDefinitionBuilder.Build();

            this.pollingDefinitions.Add(cleanupPollingDefinition);

            return this;
        }

        public PollingDefinitionsAggregatorBuilder WithRetryDurablePollingConfiguration(Action<RetryDurablePollingDefinitionBuilder> configure)
        {
            var retryDurablePollingDefinitionBuilder = new RetryDurablePollingDefinitionBuilder();
            configure(retryDurablePollingDefinitionBuilder);
            var retryDurablepollingDefinition = retryDurablePollingDefinitionBuilder.Build();

            this.pollingDefinitions.Add(retryDurablepollingDefinition);

            return this;
        }

        public PollingDefinitionsAggregatorBuilder WithSchedulerId(string schedulerId)
        {
            this.schedulerId = schedulerId;
            return this;
        }

        internal PollingDefinitionsAggregator Build()
        {
            this.FillMissingOptionalPollingDefinitions();

            return new PollingDefinitionsAggregator(this.schedulerId, this.pollingDefinitions);
        }

        private void FillMissingOptionalPollingDefinitions()
        {
            if (!this.pollingDefinitions.Any(pd => pd.PollingJobType == PollingJobType.Cleanup))
            {
                var cleanupPollingDefinition = new CleanupPollingDefinitionBuilder()
                    .Enabled(false)
                    .Build();

                this.pollingDefinitions.Add(cleanupPollingDefinition);
            }
        }
    }
}