using System;
using KafkaFlow;
using KafkaFlow.Configuration;
using KafkaFlow.Retry.Durable;
using KafkaFlow.Retry.Forever;
using KafkaFlow.Retry.Simple;

namespace KafkaFlow.Retry;

public static class ConfigurationBuilderExtensions
{
    public static IConsumerMiddlewareConfigurationBuilder RetryDurable(
        this IConsumerMiddlewareConfigurationBuilder middlewareBuilder,
        Action<RetryDurableDefinitionBuilder> configure)
    {
            var retryDurableDefinitionBuilder = new RetryDurableDefinitionBuilder();
            configure(retryDurableDefinitionBuilder);
            var retryDurableDefinition = retryDurableDefinitionBuilder.Build();

            return middlewareBuilder.Add(
                resolver => new RetryDurableMiddleware(
                    resolver.Resolve<ILogHandler>(),
                    retryDurableDefinition
                ));
        }

    public static IConsumerMiddlewareConfigurationBuilder RetryForever(
        this IConsumerMiddlewareConfigurationBuilder middlewareBuilder,
        Action<RetryForeverDefinitionBuilder> configure)
    {
            var retryForeverDefinitionBuilder = new RetryForeverDefinitionBuilder();

            configure(retryForeverDefinitionBuilder);

            return middlewareBuilder.Add(
                resolver => new RetryForeverMiddleware(
                    resolver.Resolve<ILogHandler>(),
                    retryForeverDefinitionBuilder.Build()
                ));
        }

    public static IConsumerMiddlewareConfigurationBuilder RetrySimple(
        this IConsumerMiddlewareConfigurationBuilder middlewareBuilder,
        Action<RetrySimpleDefinitionBuilder> configure)
    {
            var retryDefinitionBuilder = new RetrySimpleDefinitionBuilder();

            configure(retryDefinitionBuilder);

            return middlewareBuilder.Add(
                resolver => new RetrySimpleMiddleware(
                    resolver.Resolve<ILogHandler>(),
                    retryDefinitionBuilder.Build()
                ));
        }
}