namespace KafkaFlow.Retry
{
    using System;
    using KafkaFlow.Retry.Durable.Definitions;

    public class RetryDurableRetryPlanBeforeDefinitionBuilder
    {
        private int numberOfRetries;
        private bool pauseConsumer;
        private Func<int, TimeSpan> timeBetweenTriesPlan;

        public RetryDurableRetryPlanBeforeDefinitionBuilder ShouldPauseConsumer(bool pause)
        {
            this.pauseConsumer = pause;
            return this;
        }

        public RetryDurableRetryPlanBeforeDefinitionBuilder TryTimes(int numberOfRetries)
        {
            this.numberOfRetries = numberOfRetries;
            return this;
        }

        public RetryDurableRetryPlanBeforeDefinitionBuilder WithTimeBetweenTriesPlan(Func<int, TimeSpan> timeBetweenTriesPlan)
        {
            this.timeBetweenTriesPlan = timeBetweenTriesPlan;
            return this;
        }

        public RetryDurableRetryPlanBeforeDefinitionBuilder WithTimeBetweenTriesPlan(params TimeSpan[] timeBetweenRetries)
            => this.WithTimeBetweenTriesPlan(
                    (retryNumber) =>
                       ((retryNumber - 1) < timeBetweenRetries.Length)
                           ? timeBetweenRetries[retryNumber - 1]
                           : timeBetweenRetries[timeBetweenRetries.Length - 1]
                );

        internal RetryDurableRetryPlanBeforeDefinition Build()
        {
            return new RetryDurableRetryPlanBeforeDefinition(
                this.timeBetweenTriesPlan,
                this.numberOfRetries,
                this.pauseConsumer
            );
        }
    }
}