namespace KafkaFlow.Retry.Durable
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class KafkaRetryDurableDefinition
    {
        private readonly IReadOnlyCollection<Func<KafkaRetryContext, bool>> retryWhenExceptions;
        private readonly Func<int, TimeSpan> timeBetweenTriesPlan;

        public KafkaRetryDurableDefinition(
            Func<int, TimeSpan> timeBetweenTriesPlan,
            IReadOnlyCollection<Func<KafkaRetryContext, bool>> retryWhenExceptions
            )
        {
            if (!retryWhenExceptions.Any())
            {
                throw new ArgumentException("There is exceptions defined", nameof(retryWhenExceptions));
            }

            this.timeBetweenTriesPlan = timeBetweenTriesPlan;
            this.retryWhenExceptions = retryWhenExceptions;
        }

        internal Func<int, TimeSpan> TimeBetweenTriesPlan =>
            this.timeBetweenTriesPlan;

        internal bool ShouldRetry(KafkaRetryContext kafkaRetryContext) =>
            this.retryWhenExceptions.Any(rule => rule(kafkaRetryContext));
    }
}