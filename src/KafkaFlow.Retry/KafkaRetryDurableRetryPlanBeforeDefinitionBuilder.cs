namespace KafkaFlow.Retry
{
    using System;
    using KafkaFlow.Retry.Durable;

    public class KafkaRetryDurableRetryPlanBeforeDefinitionBuilder
    {
        private int numberOfRetries;
        private bool pauseConsumer;
        private Func<int, TimeSpan> timeBetweenTriesPlan;

        public KafkaRetryDurableRetryPlanBeforeDefinitionBuilder ShouldPauseConsumer(bool pause)
        {
            this.pauseConsumer = pause;
            return this;
        }

        public KafkaRetryDurableRetryPlanBeforeDefinitionBuilder TryTimes(int numberOfRetries)
        {
            this.numberOfRetries = numberOfRetries;
            return this;
        }

        public KafkaRetryDurableRetryPlanBeforeDefinitionBuilder WithTimeBetweenTriesPlan(Func<int, TimeSpan> timeBetweenTriesPlan)
        {
            this.timeBetweenTriesPlan = timeBetweenTriesPlan;
            return this;
        }

        public KafkaRetryDurableRetryPlanBeforeDefinitionBuilder WithTimeBetweenTriesPlan(params TimeSpan[] timeBetweenRetries)
            => this.WithTimeBetweenTriesPlan(
                    (retryNumber) =>
                       ((retryNumber - 1) < timeBetweenRetries.Length)
                           ? timeBetweenRetries[retryNumber - 1]
                           : timeBetweenRetries[timeBetweenRetries.Length - 1]
                );

        internal KafkaRetryDurableRetryPlanBeforeDefinition Build()
        {
            return new KafkaRetryDurableRetryPlanBeforeDefinition(
                this.timeBetweenTriesPlan,
                this.numberOfRetries,
                this.pauseConsumer
            );
        }
    }
}