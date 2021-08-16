namespace KafkaFlow.Retry.Durable
{
    using KafkaFlow.Configuration;
    using KafkaFlow.Retry.Durable.Encoders;
    using KafkaFlow.Retry.Durable.Repository;

    internal static class RetryDurableConsumerConfigurationBuilderExtensions
    {
        public static IConsumerMiddlewareConfigurationBuilder RetryConsumerStrategy(
            this IConsumerMiddlewareConfigurationBuilder middlewareBuilder,
            RetryConsumerStrategy retryConsumerStrategy,
            IRetryDurableQueueRepository retryDurableQueueRepository,
            IUtf8Encoder utf8Encoder)
        {
            switch (retryConsumerStrategy)
            {
                case Retry.RetryConsumerStrategy.GuaranteeOrderedConsumption:
                    {
                        middlewareBuilder.Add(
                           resolver => new RetryDurableConsumerGuaranteeOrderedMiddleware(
                               resolver.Resolve<ILogHandler>(),
                               retryDurableQueueRepository,
                               utf8Encoder
                           ));
                    }
                    break;

                case Retry.RetryConsumerStrategy.LatestConsumption:
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
                    break;
            }

            return middlewareBuilder;
        }
    }
}