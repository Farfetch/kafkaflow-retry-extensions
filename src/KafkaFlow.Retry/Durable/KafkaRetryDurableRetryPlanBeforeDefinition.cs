namespace KafkaFlow.Retry.Durable
{
    using System;

    internal class KafkaRetryDurableRetryPlanBeforeDefinition
    {
        private readonly int numberOfRetries;
        private readonly bool pauseConsumer;
        private readonly Func<int, TimeSpan> timeBetweenTriesPlan;

        public KafkaRetryDurableRetryPlanBeforeDefinition(
            Func<int, TimeSpan> timeBetweenTriesPlan,
            int numberOfRetries,
            bool pauseConsumer
            )
        {
            this.timeBetweenTriesPlan = timeBetweenTriesPlan;
            this.numberOfRetries = numberOfRetries;
            this.pauseConsumer = pauseConsumer;
        }

        internal Func<int, TimeSpan> TimeBetweenTriesPlan =>
            this.timeBetweenTriesPlan;

        internal int GetNumberOfRetries() =>
            this.numberOfRetries;

        internal bool ShouldPauseConsumer() =>
                    this.pauseConsumer;
    }
}