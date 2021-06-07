namespace KafkaFlow.Retry
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class KafkaRetryDefinition
    {
        private readonly int numberOfRetries;
        private readonly bool pauseConsumer;
        private readonly IReadOnlyCollection<Func<KafkaRetryContext, bool>> retryWhenExceptions;
        private readonly Func<int, TimeSpan> timeBetweenTriesPlan;

        public KafkaRetryDefinition(
            int numberOfRetries,
            IReadOnlyCollection<Func<KafkaRetryContext, bool>> retryWhenExceptions,
            bool pauseConsumer,
            Func<int, TimeSpan> timeBetweenTriesPlan
            )
        {
            if (numberOfRetries < 1)
            {
                throw new ArgumentException("The number of retries should be higher than zero", nameof(numberOfRetries));
            }

            if (!retryWhenExceptions.Any())
            {
                throw new ArgumentException("There is exceptions defined", nameof(retryWhenExceptions));
            }

            this.retryWhenExceptions = retryWhenExceptions;
            this.numberOfRetries = numberOfRetries;
            this.pauseConsumer = pauseConsumer;
            this.timeBetweenTriesPlan = timeBetweenTriesPlan;
        }

        internal Func<int, TimeSpan> TimeBetweenTriesPlan =>
            this.timeBetweenTriesPlan;

        internal int GetNumberOfRetries() => // TODO: code guideline: use a property { get; } here
                    this.numberOfRetries;

        internal bool ShouldPauseConsumer() => // TODO: code guideline: use a property { get; } here
                    this.pauseConsumer;

        internal bool ShouldRetry(KafkaRetryContext kafkaRetryContext) =>
            this.retryWhenExceptions.Any(rule => rule(kafkaRetryContext));
    }
}