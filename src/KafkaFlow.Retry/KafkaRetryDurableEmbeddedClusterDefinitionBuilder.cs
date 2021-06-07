namespace KafkaFlow.Retry
{
    using System;
    using System.Linq;
    using System.Threading;
    using KafkaFlow.Compressor;
    using KafkaFlow.Compressor.Gzip;
    using KafkaFlow.Configuration;
    using KafkaFlow.Retry.Durable;
    using KafkaFlow.Retry.Durable.Polling;
    using KafkaFlow.Serializer;
    using KafkaFlow.Serializer.NewtonsoftJson;
    using KafkaFlow.TypedHandler;

    public class KafkaRetryDurableEmbeddedClusterDefinitionBuilder
    {
        private const int DefaultPartitionElection = 0;
        private readonly IClusterConfigurationBuilder cluster;
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
                    KafkaRetryDurableConstants.EmbeddedProducerName,
                    producer => producer
                        .DefaultTopic(this.retryTopicName)
                        .AddMiddlewares(
                            middlewares => middlewares
                                .AddSerializer<NewtonsoftJsonMessageSerializer>()
                                .AddCompressor<GzipMessageCompressor>()
                        )
                        .WithAcks(Acks.All)
                )
                .AddConsumer(
                    consumer => consumer
                        .Topic(this.retryTopicName)
                        .WithGroupId(KafkaRetryDurableConstants.EmbeddedConsumerGroupId)
                        .WithName(KafkaRetryDurableConstants.EmbeddedConsumerName)
                        .WithBufferSize(10)
                        .WithWorkersCount(20)
                        .WithAutoOffsetReset(AutoOffsetReset.Latest)
                        .WithPartitionsAssignedHandler(
                            async (resolver, partitionsAssignedHandler) =>
                            {
                                if (partitionsAssignedHandler is object
                                 && partitionsAssignedHandler.Any(tp => tp.Partition == DefaultPartitionElection))
                                {
                                    var queueTrackerCoordinator = resolver.Resolve<IQueueTrackerCoordinator>();
                                    await queueTrackerCoordinator
                                            .InitializeAsync(CancellationToken.None) // TODO: this will probably cause problems during the app shutdown. Handles cannot stay open. we need a cancellation token, event if it is only for internal usage
                                            .ConfigureAwait(false);
                                }
                            })
                        .WithPartitionsRevokedHandler(
                            async (resolver, partitionsRevokedHandler) =>
                            {
                                var queueTrackerCoordinator = resolver.Resolve<IQueueTrackerCoordinator>();
                                await queueTrackerCoordinator
                                        .ShutdownAsync(CancellationToken.None) /// same
                                        .ConfigureAwait(false);
                            })
                        .AddMiddlewares(
                            middlewares => middlewares
                                .AddCompressor<GzipMessageCompressor>()
                                .AddSerializer<NewtonsoftJsonMessageSerializer>() // I think we should use a better serializer for binary (key;mesage), pls check but I think protobuff is a better option
                                .Add<KafkaRetryDurableValidationMiddleware>()
                                .AddTypedHandlers(this.typeHandlers)
                        )
                );
        }
    }
}