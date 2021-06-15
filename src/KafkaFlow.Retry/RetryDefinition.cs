namespace KafkaFlow.Retry
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Dawn;

    internal class RetryDefinition
    {
        private readonly IReadOnlyCollection<Func<RetryContext, bool>> retryWhenExceptions;

        public RetryDefinition(
            int numberOfRetries,
            IReadOnlyCollection<Func<RetryContext, bool>> retryWhenExceptions,
            bool pauseConsumer,
            Func<int, TimeSpan> timeBetweenTriesPlan
            )
        {
            Guard.Argument(numberOfRetries).NotZero().NotNegative(value => "The number of retries should be higher than zero");
            Guard.Argument(retryWhenExceptions).NotNull("At least an exception should be defined");
            Guard.Argument(retryWhenExceptions.Count).NotNegative(value => "At least an exception should be defined");
            Guard.Argument(timeBetweenTriesPlan).NotNull("A plan of times betwwen tries should be defined");

            this.retryWhenExceptions = retryWhenExceptions;
            this.TimeBetweenTriesPlan = timeBetweenTriesPlan;
            this.NumberOfRetries = numberOfRetries;
            this.PauseConsumer = pauseConsumer;
        }

        public int NumberOfRetries { get; }

        public bool PauseConsumer { get; }

        internal Func<int, TimeSpan> TimeBetweenTriesPlan { get; }

        internal bool ShouldRetry(RetryContext kafkaRetryContext) =>
            this.retryWhenExceptions.Any(rule => rule(kafkaRetryContext));
    }
}