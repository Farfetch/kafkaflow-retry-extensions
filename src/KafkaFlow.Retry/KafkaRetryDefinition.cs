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
        private readonly IReadOnlyCollection<TimeSpan> timesBetweenRetries;

        public KafkaRetryDefinition(
            int numberOfRetries,
            IReadOnlyCollection<TimeSpan> timesBetweenRetries,
            IReadOnlyCollection<Func<KafkaRetryContext, bool>> retryWhenExceptions,
            bool pauseConsumer
            )
        {
            if (!timesBetweenRetries.Any())
            {
                throw new ArgumentException("There is no time between retries defined", nameof(timesBetweenRetries));
            }

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
            this.timesBetweenRetries = timesBetweenRetries;
            this.pauseConsumer = pauseConsumer;
        }

        internal int GetNumberOfRetries() =>
            this.numberOfRetries;

        internal TimeSpan GetTimeSpanAtOrLast(int retryNumber) =>
                ((retryNumber - 1) < timesBetweenRetries.Count)
            ? this.timesBetweenRetries.ElementAt(retryNumber - 1)
            : this.timesBetweenRetries.Last();

        internal bool ShouldPauseConsumer() =>
            this.pauseConsumer;

        internal bool ShouldRetry(KafkaRetryContext kafkaRetryContext) =>
            this.retryWhenExceptions.Any(rule => rule(kafkaRetryContext));
    }
}