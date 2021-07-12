namespace KafkaFlow.Retry.Durable.Definitions
{
    using System;
    using Dawn;

    internal class RetryDurableRetryPlanBeforeDefinition : IRetryDurableRetryPlanBeforeDefinition
    {
        public RetryDurableRetryPlanBeforeDefinition(
            Func<int, TimeSpan> timeBetweenTriesPlan,
            int numberOfRetries,
            bool pauseConsumer
            )
        {
            Guard.Argument(numberOfRetries).NotZero().NotNegative(value => "The number of retries should be higher than zero");
            Guard.Argument(timeBetweenTriesPlan).NotNull("A plan of times betwwen tries should be defined");

            this.TimeBetweenTriesPlan = timeBetweenTriesPlan;
            this.NumberOfRetries = numberOfRetries;
            this.PauseConsumer = pauseConsumer;
        }

        public int NumberOfRetries { get; }

        public bool PauseConsumer { get; }

        public Func<int, TimeSpan> TimeBetweenTriesPlan { get; }
    }
}