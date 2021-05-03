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
            string cronExpression)
        {
            this.retryWhenExceptions = retryWhenExceptions;
            CronExpression = cronExpression;
        }

        public string CronExpression { get; }

        internal bool ShouldRetry(KafkaRetryContext kafkaRetryContext) =>
            this.retryWhenExceptions.Any(rule => rule(kafkaRetryContext));
    }
}