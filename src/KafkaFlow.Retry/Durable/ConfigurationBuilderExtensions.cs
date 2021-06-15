namespace KafkaFlow.Retry.Durable
{
    using KafkaFlow.Configuration;
    using KafkaFlow.Retry.Durable.Encoders;
    using KafkaFlow.Retry.Durable.Repository;

    internal static class ConfigurationBuilderExtensions
    {
        public static IConsumerMiddlewareConfigurationBuilder RetryConsumerStrategy(
          this IConsumerMiddlewareConfigurationBuilder middlewareBuilder,
          RetryConsumerStrategy retryConsumerStrategy)
        {
            switch (retryConsumerStrategy)
            {
                case Retry.RetryConsumerStrategy.GuaranteeOrderedConsumption:
                    {
                        middlewareBuilder.Add(
                           resolver => new RetryDurableConsumerGuaranteeOrderedMiddleware(
                               resolver.Resolve<ILogHandler>(),
                               resolver.Resolve<IRetryDurableQueueRepository>(),
                               resolver.Resolve<IUtf8Encoder>()
                           ));
                    }
                    break;

                case Retry.RetryConsumerStrategy.LatestConsumption:
                    {
                        middlewareBuilder.Add(
                           resolver => new RetryDurableConsumerLatestMiddleware(
                               resolver.Resolve<ILogHandler>(),
                               resolver.Resolve<IRetryDurableQueueRepository>(),
                               resolver.Resolve<IUtf8Encoder>()
                           ));
                    }
                    break;

                default:
                    break;
            }

            return middlewareBuilder;
        }
    }
}