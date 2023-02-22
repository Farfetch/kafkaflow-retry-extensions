namespace KafkaFlow.Retry
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Dawn;
    using KafkaFlow.Retry.Durable.Definitions.Polling;

    public class PollingDefinitionsAggregatorBuilder
    {
        private readonly CleanupPollingDefinitionBuilder cleanupPollingDefinitionBuilder;
        private readonly List<PollingDefinition> pollingDefinitions;
        private readonly RetryDurablePollingDefinitionBuilder retryDurablePollingDefinitionBuilder;
        private string schedulerId;

        public PollingDefinitionsAggregatorBuilder()
        {
            this.cleanupPollingDefinitionBuilder = new CleanupPollingDefinitionBuilder();
            this.retryDurablePollingDefinitionBuilder = new RetryDurablePollingDefinitionBuilder();

            this.pollingDefinitions = new List<PollingDefinition>();
        }

        public PollingDefinitionsAggregatorBuilder WithCleanupPollingConfiguration(Action<CleanupPollingDefinitionBuilder> configure)
        {
            configure(this.cleanupPollingDefinitionBuilder);
            var cleanupPollingDefinition = this.cleanupPollingDefinitionBuilder.Build();

            this.pollingDefinitions.Add(cleanupPollingDefinition);

            return this;
        }

        public PollingDefinitionsAggregatorBuilder WithRetryDurablePollingConfiguration(Action<RetryDurablePollingDefinitionBuilder> configure)
        {
            configure(this.retryDurablePollingDefinitionBuilder);
            var retryDurablepollingDefinition = this.retryDurablePollingDefinitionBuilder.Build();

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
            if (this.retryDurablePollingDefinitionBuilder.Required)
            {
                this.ValidateRequiredPollingDefinition(PollingJobType.RetryDurable);
            }

            if (this.cleanupPollingDefinitionBuilder.Required)
            {
                this.ValidateRequiredPollingDefinition(PollingJobType.Cleanup);
            }

            return new PollingDefinitionsAggregator(this.schedulerId, this.pollingDefinitions);
        }

        private void ValidateRequiredPollingDefinition(PollingJobType pollingJobType)
        {
            Guard.Argument(this.pollingDefinitions.Any(pd => pd.PollingJobType == pollingJobType), nameof(this.pollingDefinitions))
                 .True($"The polling job {pollingJobType} must be defined.");
        }
    }
}