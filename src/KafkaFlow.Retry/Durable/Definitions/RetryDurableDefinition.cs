namespace KafkaFlow.Retry.Durable.Definitions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Dawn;
    using KafkaFlow.Retry.Durable.Repository;

    internal class RetryDurableDefinition
    {
        private readonly IReadOnlyCollection<Func<RetryContext, bool>> retryWhenExceptions;

        public RetryDurableDefinition(
            IReadOnlyCollection<Func<RetryContext, bool>> retryWhenExceptions,
            RetryDurableRetryPlanBeforeDefinition retryDurableRetryPlanBeforeDefinition,
            IRetryDurableQueueRepository retryDurableQueueRepository)
        {
            Guard.Argument(retryWhenExceptions).NotNull("At least an exception should be defined");
            Guard.Argument(retryWhenExceptions.Count).NotNegative(value => "At least an exception should be defined");
            Guard.Argument(retryDurableRetryPlanBeforeDefinition).NotNull();
            Guard.Argument(retryDurableQueueRepository).NotNull();

            this.retryWhenExceptions = retryWhenExceptions;
            this.RetryDurableRetryPlanBeforeDefinition = retryDurableRetryPlanBeforeDefinition;
            this.RetryDurableQueueRepository = retryDurableQueueRepository;
        }

        public IRetryDurableQueueRepository RetryDurableQueueRepository { get; }

        public RetryDurableRetryPlanBeforeDefinition RetryDurableRetryPlanBeforeDefinition { get; }

        public bool ShouldRetry(RetryContext kafkaRetryContext) =>
            this.retryWhenExceptions.Any(rule => rule(kafkaRetryContext));
    }
}