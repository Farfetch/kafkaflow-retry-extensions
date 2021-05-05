namespace KafkaFlow.Retry.Durable
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using KafkaFlow.Retry.Durable.Repository;

    internal class KafkaRetryDurableDefinition
    {
        private readonly IReadOnlyCollection<Func<KafkaRetryContext, bool>> retryWhenExceptions;

        public KafkaRetryDurableDefinition(
            IReadOnlyCollection<Func<KafkaRetryContext, bool>> retryWhenExceptions,
            string cronExpression,
            IRetryQueueStorage retryQueueStorage,
            KafkaRetryDurableRetryPlanBeforeDefinition kafkaRetryDurableRetryPlanBeforeDefinition)
        {
            this.retryWhenExceptions = retryWhenExceptions;
            CronExpression = cronExpression;
            RetryQueueStorage = retryQueueStorage;
            KafkaRetryDurableRetryPlanBeforeDefinition = kafkaRetryDurableRetryPlanBeforeDefinition;
        }

        public string CronExpression { get; }
        public KafkaRetryDurableRetryPlanBeforeDefinition KafkaRetryDurableRetryPlanBeforeDefinition { get; }
        public IRetryQueueStorage RetryQueueStorage { get; }

        internal bool ShouldRetry(KafkaRetryContext kafkaRetryContext) =>
            this.retryWhenExceptions.Any(rule => rule(kafkaRetryContext));
    }
}