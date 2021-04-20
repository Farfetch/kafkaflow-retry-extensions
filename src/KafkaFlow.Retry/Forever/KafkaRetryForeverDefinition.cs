namespace KafkaFlow.Retry.Forever
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class KafkaRetryForeverDefinition
    {
        private readonly int numberOfRetries;
        private readonly IReadOnlyCollection<Func<KafkaRetryContext, bool>> retryWhenExceptions;
        private readonly IReadOnlyCollection<TimeSpan> timesBetweenRetries;

        public KafkaRetryForeverDefinition(
            IReadOnlyCollection<TimeSpan> timesBetweenRetries,
            IReadOnlyCollection<Func<KafkaRetryContext, bool>> retryWhenExceptions
            )
        {
            if (!timesBetweenRetries.Any())
            {
                throw new ArgumentException("There is no time between retries defined", nameof(timesBetweenRetries));
            }

            if (!retryWhenExceptions.Any())
            {
                throw new ArgumentException("There is exceptions defined", nameof(retryWhenExceptions));
            }

            this.retryWhenExceptions = retryWhenExceptions;
            this.timesBetweenRetries = timesBetweenRetries;
        }

        internal TimeSpan GetTimeSpanAtOrLast(int retryNumber) =>
                ((retryNumber - 1) < timesBetweenRetries.Count)
            ? this.timesBetweenRetries.ElementAt(retryNumber - 1)
            : this.timesBetweenRetries.Last();

        internal bool ShouldRetry(KafkaRetryContext kafkaRetryContext) =>
            this.retryWhenExceptions.Any(rule => rule(kafkaRetryContext));
    }
}