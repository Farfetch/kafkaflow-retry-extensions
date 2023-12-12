using System;
using KafkaFlow.Configuration;
using KafkaFlow.Retry.Durable.Encoders;
using KafkaFlow.Retry.Durable.Repository;

namespace KafkaFlow.Retry.Durable;

internal static class RetryDurableConsumerConfigurationBuilderExtensions
{
    public static IConsumerMiddlewareConfigurationBuilder WithRetryConsumerStrategy(
        this IConsumerMiddlewareConfigurationBuilder middlewareBuilder,
        RetryConsumerStrategy retryConsumerStrategy,
        IRetryDurableQueueRepository retryDurableQueueRepository,
        IUtf8Encoder utf8Encoder)
    {
        switch (retryConsumerStrategy)
        {
            case RetryConsumerStrategy.GuaranteeOrderedConsumption:
            {
                middlewareBuilder.Add(
                    resolver => new RetryDurableConsumerGuaranteeOrderedMiddleware(
                        resolver.Resolve<ILogHandler>(),
                        retryDurableQueueRepository,
                        utf8Encoder
                    ));
            }
                break;

            case RetryConsumerStrategy.LatestConsumption:
            {
                middlewareBuilder.Add(
                    resolver => new RetryDurableConsumerLatestMiddleware(
                        resolver.Resolve<ILogHandler>(),
                        retryDurableQueueRepository,
                        utf8Encoder
                    ));
            }
                break;

            default:
                throw new NotImplementedException($"{nameof(RetryConsumerStrategy)} not defined");
        }

        return middlewareBuilder;
    }
}