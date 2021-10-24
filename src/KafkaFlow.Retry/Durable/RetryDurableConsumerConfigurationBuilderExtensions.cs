namespace KafkaFlow.Retry.Durable
{
    using System;
    using KafkaFlow.Configuration;
    using KafkaFlow.Retry.Durable.Encoders;
    using KafkaFlow.Retry.Durable.Repository;
    using KafkaFlow.Retry.Durable.Serializers;

    internal static class RetryDurableConsumerConfigurationBuilderExtensions
    {
        public static IConsumerMiddlewareConfigurationBuilder WithMessageSerializerStrategy(
            this IConsumerMiddlewareConfigurationBuilder middlewareBuilder,
            MessageSerializerStrategy messageSerializerStrategy,
            Type messageType,
            IUtf8Encoder utf8Encoder,
            INewtonsoftJsonSerializer newtonsoftJsonSerializer)
        {
            switch (messageSerializerStrategy)
            {
                case MessageSerializerStrategy.NewtonsoftJson:
                    {
                        middlewareBuilder
                            .Add(resolver => new RetryDurableConsumerUtf8EncoderMiddleware(utf8Encoder))
                            .Add(resolver => new RetryDurableConsumerNewtonsoftJsonSerializerMiddleware(newtonsoftJsonSerializer, messageType));
                    }
                    break;

                default:
                    throw new NotImplementedException($"{nameof(MessageSerializerStrategy)} not defined");
            }

            return middlewareBuilder;
        }

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
}