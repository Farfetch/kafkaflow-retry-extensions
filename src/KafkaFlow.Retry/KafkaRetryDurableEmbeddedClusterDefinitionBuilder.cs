namespace KafkaFlow.Retry
{
    using System;
    using KafkaFlow.Compressor;
    using KafkaFlow.Compressor.Gzip;
    using KafkaFlow.Configuration;
    using KafkaFlow.Retry.Durable;
    using KafkaFlow.Serializer;
    using KafkaFlow.Serializer.ProtoBuf;
    using KafkaFlow.TypedHandler;

    public class KafkaRetryDurableEmbeddedClusterDefinitionBuilder
    {
        private IClusterConfigurationBuilder cluster;
        private bool enabled;
        private string retryTopicName;
        private Action<TypedHandlerConfigurationBuilder> typeHandlers;

        public KafkaRetryDurableEmbeddedClusterDefinitionBuilder(IClusterConfigurationBuilder cluster)
        {
            this.cluster = cluster;
        }

        public KafkaRetryDurableEmbeddedClusterDefinitionBuilder Enabled(bool enabled)
        {
            this.enabled = enabled;
            return this;
        }

        public KafkaRetryDurableEmbeddedClusterDefinitionBuilder WithRetryTopicName(string retryTopicName)
        {
            this.retryTopicName = retryTopicName;
            return this;
        }

        public KafkaRetryDurableEmbeddedClusterDefinitionBuilder WithTypedHandlers(Action<TypedHandlerConfigurationBuilder> typeHandlers)
        {
            this.typeHandlers = typeHandlers;
            return this;
        }

        internal void Build()
        {
            if (!enabled)
            {
                return;
            }

            this.cluster
                .AddProducer(
                    "kafka-flow-retry-durable-producer",
                    producer => producer
                        .DefaultTopic(this.retryTopicName)
                        .AddMiddlewares(
                            middlewares => middlewares
                        .AddSerializer<ProtobufMessageSerializer>()
                        .AddCompressor<GzipMessageCompressor>()
                        )
                        .WithAcks(Acks.All)
                )
                .AddConsumer(
                    consumer => consumer
                        .Topic(this.retryTopicName)
                        .WithGroupId("kafka-flow-retry-durable-consumer")
                        .WithName("kafka-flow-retry-durable-consumer")
                        .WithBufferSize(10)
                        .WithWorkersCount(20)
                        .WithAutoOffsetReset(AutoOffsetReset.Latest)
                        .AddMiddlewares(
                            middlewares => middlewares
                                .AddCompressor<GzipMessageCompressor>()
                                .AddSerializer<ProtobufMessageSerializer>()
                                .Add<KafkaRetryDurableValidationMiddleware>()
                                .AddTypedHandlers(this.typeHandlers)
                        )
                );
        }
    }
}