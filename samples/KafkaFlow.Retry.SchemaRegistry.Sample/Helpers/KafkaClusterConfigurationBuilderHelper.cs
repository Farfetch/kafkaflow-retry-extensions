namespace KafkaFlow.Retry.SchemaRegistry.Sample.Helpers
{
    using System;
    using Confluent.SchemaRegistry;
    using Confluent.SchemaRegistry.Serdes;
    using global::SchemaRegistry;
    using KafkaFlow.Configuration;
    using KafkaFlow.Retry.MongoDb;
    using KafkaFlow.Retry.SchemaRegistry.Sample.ContractResolvers;
    using KafkaFlow.Retry.SchemaRegistry.Sample.Exceptions;
    using KafkaFlow.Retry.SchemaRegistry.Sample.Handlers;
    using KafkaFlow.TypedHandler;
    using Newtonsoft.Json;

    internal static class KafkaClusterConfigurationBuilderHelper
    {
        internal static IClusterConfigurationBuilder SetupRetryDurableMongoAvroDb(
            this IClusterConfigurationBuilder cluster,
            string mongoDbConnectionString,
            string mongoDbDatabaseName,
            string mongoDbRetryQueueCollectionName,
            string mongoDbRetryQueueItemCollectionName)
        {
            cluster
                .AddProducer(
                    "kafka-flow-retry-durable-mongodb-avro-producer",
                    producer => producer
                        .DefaultTopic("sample-kafka-flow-retry-durable-mongodb-avro-topic")
                        .WithCompression(Confluent.Kafka.CompressionType.Gzip)
                        .AddMiddlewares(
                            middlewares => middlewares
                                .AddSchemaRegistryAvroSerializer(
                                    new AvroSerializerConfig
                                    {
                                        SubjectNameStrategy = SubjectNameStrategy.TopicRecord
                                    })
                        )
                        .WithAcks(Acks.All)
                )
                .AddConsumer(
                    consumer => consumer
                        .Topic("sample-kafka-flow-retry-durable-mongodb-avro-topic")
                        .WithGroupId("sample-consumer-kafka-flow-retry-durable-mongodb-avro")
                        .WithName("kafka-flow-retry-durable-mongodb-avro-consumer")
                        .WithBufferSize(10)
                        .WithWorkersCount(20)
                        .WithAutoOffsetReset(AutoOffsetReset.Latest)
                        .AddMiddlewares(
                            middlewares => middlewares
                                .AddSchemaRegistryAvroSerializer()
                                .RetryDurable(
                                    configure => configure
                                        .Handle<RetryDurableTestException>()
                                        .WithMessageType(typeof(AvroLogMessage))
                                        .WithMessageSerializeSettings(new JsonSerializerSettings
                                        {
                                            ContractResolver = new WritablePropertiesOnlyResolver()
                                        })
                                        .WithMongoDbDataProvider(
                                            mongoDbConnectionString,
                                            mongoDbDatabaseName,
                                            mongoDbRetryQueueCollectionName,
                                            mongoDbRetryQueueItemCollectionName)
                                        .WithRetryPlanBeforeRetryDurable(
                                            configure => configure
                                                .TryTimes(3)
                                                .WithTimeBetweenTriesPlan(
                                                    TimeSpan.FromMilliseconds(250),
                                                    TimeSpan.FromMilliseconds(500),
                                                    TimeSpan.FromMilliseconds(1000))
                                                .ShouldPauseConsumer(false)
                                        )
                                        .WithEmbeddedRetryCluster(
                                            cluster,
                                            configure => configure
                                                .WithRetryTopicName("sample-kafka-flow-retry-durable-mongodb-avro-topic-retry")
                                                .WithRetryConsumerBufferSize(4)
                                                .WithRetryConsumerWorkersCount(2)
                                                .WithRetryConsumerStrategy(RetryConsumerStrategy.GuaranteeOrderedConsumption)
                                                .WithRetryTypedHandlers(
                                                    handlers => handlers
                                                        .WithHandlerLifetime(InstanceLifetime.Transient)
                                                        .AddHandler<AvroMessageTestHandler>()
                                                )
                                                .Enabled(true)
                                        )
                                        .WithQueuePollingJobConfiguration(
                                            configure => configure
                                                .WithId("retry-durable-mongodb-avro-polling-id")
                                                .WithCronExpression("0 0/1 * 1/1 * ? *")
                                                .WithExpirationIntervalFactor(1)
                                                .WithFetchSize(10)
                                                .Enabled(true)
                                        ))
                                .AddTypedHandlers(
                                    handlers => handlers
                                        .WithHandlerLifetime(InstanceLifetime.Transient)
                                        .AddHandler<AvroMessageTestHandler>())
                        )
                );

            return cluster;
        }
    }
}