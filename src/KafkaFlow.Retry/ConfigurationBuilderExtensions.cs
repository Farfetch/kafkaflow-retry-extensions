namespace KafkaFlow.Retry
{
    using System;
    using KafkaFlow;
    using KafkaFlow.Configuration;
    using KafkaFlow.Producers;
    using KafkaFlow.Retry.Durable;
    using KafkaFlow.Retry.Forever;

    public static class ConfigurationBuilderExtensions
    {
        public static IConsumerMiddlewareConfigurationBuilder Retry(
               this IConsumerMiddlewareConfigurationBuilder middlewareBuilder,
               Action<KafkaRetryDefinitionBuilder> configure)
        {
            var kafkaRetryDefinitionBuilder = new KafkaRetryDefinitionBuilder();

            configure(kafkaRetryDefinitionBuilder);

            return middlewareBuilder.Add(
                resolver => new KafkaRetryMiddleware(
                    resolver.Resolve<ILogHandler>(),
                    kafkaRetryDefinitionBuilder.Build()
                ));
        }

        public static IConsumerMiddlewareConfigurationBuilder RetryDurable(
                       this IConsumerMiddlewareConfigurationBuilder middlewareBuilder,
               Action<KafkaRetryDurableDefinitionBuilder> configure)
        {
            var kafkaRetryDurableDefinitionBuilder = new KafkaRetryDurableDefinitionBuilder();

            configure(kafkaRetryDurableDefinitionBuilder);

            var kafkaRetryDurableDefinitionBuild = kafkaRetryDurableDefinitionBuilder.Build();

            return middlewareBuilder.Add(
                resolver => new KafkaRetryDurableMiddleware(
                    resolver.Resolve<ILogHandler>(),
                    kafkaRetryDurableDefinitionBuild,
                    resolver.Resolve<IProducerAccessor>()
                ));
        }

        public static IConsumerMiddlewareConfigurationBuilder RetryForever(
                               this IConsumerMiddlewareConfigurationBuilder middlewareBuilder,
               Action<KafkaRetryForeverDefinitionBuilder> configure)
        {
            var kafkaRetryForeverDefinitionBuilder = new KafkaRetryForeverDefinitionBuilder();

            configure(kafkaRetryForeverDefinitionBuilder);

            return middlewareBuilder.Add(
                resolver => new KafkaRetryForeverMiddleware(
                    resolver.Resolve<ILogHandler>(),
                    kafkaRetryForeverDefinitionBuilder.Build()
                ));
        }
    }
}