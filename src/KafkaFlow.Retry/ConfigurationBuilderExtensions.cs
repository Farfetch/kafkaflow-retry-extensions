namespace KafkaFlow.Retry
{
    using System;
    using KafkaFlow;
    using KafkaFlow.Configuration;
    using KafkaFlow.Retry.Forever;

    public static class ConfigurationBuilderExtensions
    {
        public static IConsumerMiddlewareConfigurationBuilder RetryForeverWhen(
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

        public static IConsumerMiddlewareConfigurationBuilder RetryWhen(
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
    }
}