namespace KafkaFlow.Retry.Durable
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class KafkaRetryDurableDefinition
    {
        private readonly IReadOnlyCollection<Func<KafkaRetryContext, bool>> retryWhenExceptions;

        public KafkaRetryDurableDefinition(
            IReadOnlyCollection<Func<KafkaRetryContext, bool>> retryWhenExceptions,
            KafkaRetryDurableRetryPlanBeforeDefinition kafkaRetryDurableRetryPlanBeforeDefinition,
            KafkaRetryDurablePollingDefinition kafkaRetryDurablePollingDefinition)
        {
            this.retryWhenExceptions = retryWhenExceptions;
            KafkaRetryDurableRetryPlanBeforeDefinition = kafkaRetryDurableRetryPlanBeforeDefinition;
            KafkaRetryDurablePollingDefinition = kafkaRetryDurablePollingDefinition;
        }

        public KafkaRetryDurablePollingDefinition KafkaRetryDurablePollingDefinition { get; }
        public KafkaRetryDurableRetryPlanBeforeDefinition KafkaRetryDurableRetryPlanBeforeDefinition { get; }

        internal bool ShouldRetry(KafkaRetryContext kafkaRetryContext) =>
            this.retryWhenExceptions.Any(rule => rule(kafkaRetryContext));
    }
}