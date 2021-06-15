namespace KafkaFlow.Retry.Durable.Definitions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Dawn;

    internal class RetryDurableDefinition : IRetryDurableDefinition
    {
        private readonly IReadOnlyCollection<Func<RetryContext, bool>> retryWhenExceptions;

        public RetryDurableDefinition(
            IReadOnlyCollection<Func<RetryContext, bool>> retryWhenExceptions,
            RetryDurableRetryPlanBeforeDefinition retryDurableRetryPlanBeforeDefinition,
            RetryDurablePollingDefinition retryDurablePollingDefinition)
        {
            Guard.Argument(retryWhenExceptions).NotNull("At least an exception should be defined");
            Guard.Argument(retryWhenExceptions.Count).NotNegative(value => "At least an exception should be defined");
            Guard.Argument(retryDurableRetryPlanBeforeDefinition).NotNull();
            Guard.Argument(retryDurablePollingDefinition).NotNull();

            this.retryWhenExceptions = retryWhenExceptions;
            this.RetryDurableRetryPlanBeforeDefinition = retryDurableRetryPlanBeforeDefinition;
            this.RetryDurablePollingDefinition = retryDurablePollingDefinition;
        }

        public RetryDurablePollingDefinition RetryDurablePollingDefinition { get; }

        public RetryDurableRetryPlanBeforeDefinition RetryDurableRetryPlanBeforeDefinition { get; }

        public bool ShouldRetry(RetryContext kafkaRetryContext) =>
            this.retryWhenExceptions.Any(rule => rule(kafkaRetryContext));
    }
}