namespace KafkaFlow.Retry
{
    using System;
    using System.Collections.Generic;
    using KafkaFlow.Configuration;
    using KafkaFlow.Retry.Durable;

    public class KafkaRetryDurableDefinitionBuilder
    {
        private readonly List<Func<KafkaRetryContext, bool>> retryWhenExceptions = new List<Func<KafkaRetryContext, bool>>();
        private string cronExpression;

        public KafkaRetryDurableDefinitionBuilder Handle<TException>()
            where TException : Exception
            => this.Handle(kafkaRetryContext => kafkaRetryContext.Exception is TException);

        public KafkaRetryDurableDefinitionBuilder Handle<TException>(Func<TException, bool> rule)
            where TException : Exception
            => this.Handle(context => context.Exception is TException ex && rule(ex));

        public KafkaRetryDurableDefinitionBuilder Handle(Func<KafkaRetryContext, bool> func)
        {
            this.retryWhenExceptions.Add(func);
            return this;
        }

        public KafkaRetryDurableDefinitionBuilder HandleAnyException()
            => this.Handle(kafkaRetryContext => true);

        public KafkaRetryDurableDefinitionBuilder WithCronExpression(string cronExpression)
        {
            this.cronExpression = cronExpression;
            return this;
        }

        public KafkaRetryDurableDefinitionBuilder WithEmbeddedCluster(
            IClusterConfigurationBuilder cluster,
            Action<KafkaRetryDurableEmbeddedClusterDefinitionBuilder> configure
            )
        {
            var kafkaRetryDurableEmbeddedClusterBuilder = new KafkaRetryDurableEmbeddedClusterDefinitionBuilder(cluster);
            configure(kafkaRetryDurableEmbeddedClusterBuilder);
            kafkaRetryDurableEmbeddedClusterBuilder.Build();
            return this;
        }

        internal KafkaRetryDurableDefinition Build()
        {
            return new KafkaRetryDurableDefinition(
                this.retryWhenExceptions,
                this.cronExpression
                );
        }
    }
}