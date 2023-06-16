﻿namespace KafkaFlow.Retry.Sample.Helpers
{
    using System;
    using KafkaFlow.Configuration;
    using KafkaFlow.Retry.MongoDb;
    using KafkaFlow.Retry.Sample.Exceptions;
    using KafkaFlow.Retry.Sample.Handlers;
    using KafkaFlow.Retry.Sample.Messages;
    using KafkaFlow.Retry.SqlServer;
    using KafkaFlow.Serializer;
    using KafkaFlow.TypedHandler;

    internal static class KafkaClusterConfigurationBuilderHelper
    {
        internal static IClusterConfigurationBuilder SetupRetryDurableMongoDb(
            this IClusterConfigurationBuilder cluster,
            string mongoDbConnectionString,
            string mongoDbDatabaseName,
            string mongoDbRetryQueueCollectionName,
            string mongoDbRetryQueueItemCollectionName)
        {
            cluster
                .AddProducer(
                    "kafka-flow-retry-durable-mongodb-producer",
                    producer => producer
                        .DefaultTopic("sample-kafka-flow-retry-durable-mongodb-topic")
                        .WithCompression(Confluent.Kafka.CompressionType.Gzip)
                        .AddMiddlewares(
                            middlewares => middlewares
                                .AddSerializer<ProtobufNetSerializer>()
                        )
                        .WithAcks(Acks.All)
                )
                .AddConsumer(
                    consumer => consumer
                        .Topic("sample-kafka-flow-retry-durable-mongodb-topic")
                        .WithGroupId("sample-consumer-kafka-flow-retry-durable-mongodb")
                        .WithName("kafka-flow-retry-durable-mongodb-consumer")
                        .WithBufferSize(10)
                        .WithWorkersCount(20)
                        .WithAutoOffsetReset(AutoOffsetReset.Latest)
                        .AddMiddlewares(
                            middlewares => middlewares
                                .AddSerializer<ProtobufNetSerializer>()
                                .RetryDurable(
                                    configure => configure
                                        .Handle<RetryDurableTestException>()
                                        .WithMessageType(typeof(RetryDurableTestMessage))
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
                                                .WithRetryTopicName("sample-kafka-flow-retry-durable-mongodb-topic-retry")
                                                .WithRetryConsumerBufferSize(4)
                                                .WithRetryConsumerWorkersCount(2)
                                                .WithRetryConsumerStrategy(RetryConsumerStrategy.GuaranteeOrderedConsumption)
                                                .WithRetryTypedHandlers(
                                                    handlers => handlers
                                                        .WithHandlerLifetime(InstanceLifetime.Transient)
                                                        .AddHandler<RetryDurableTestHandler>()
                                                )
                                                .Enabled(true)
                                        )
                                        .WithQueuePollingJobConfiguration(
                                            configure => configure
                                                .WithId("retry-durable-mongodb-polling-id")
                                                .WithCronExpression("0 0/1 * 1/1 * ? *")
                                                .WithExpirationIntervalFactor(1)
                                                .WithFetchSize(10)
                                                .Enabled(true)
                                        ))
                                .AddTypedHandlers(
                                    handlers => handlers
                                        .WithHandlerLifetime(InstanceLifetime.Transient)
                                        .AddHandler<RetryDurableTestHandler>())
                        )
                );

            return cluster;
        }

        internal static IClusterConfigurationBuilder SetupRetryDurableSqlServer(
            this IClusterConfigurationBuilder cluster,
            string sqlServerConnectionString,
            string sqlServerDatabaseName)
        {
            cluster
                .AddProducer(
                    "kafka-flow-retry-durable-sqlserver-producer",
                    producer => producer
                        .DefaultTopic("sample-kafka-flow-retry-durable-sqlserver-topic")
                        .WithCompression(Confluent.Kafka.CompressionType.Gzip)
                        .AddMiddlewares(
                            middlewares => middlewares
                                .AddSerializer<ProtobufNetSerializer>()
                        )
                        .WithAcks(Acks.All)
                )
                .AddConsumer(
                    consumer => consumer
                        .Topic("sample-kafka-flow-retry-durable-sqlserver-topic")
                        .WithGroupId("sample-consumer-kafka-flow-retry-durable-sqlserver")
                        .WithName("kafka-flow-retry-durable-sqlserver-consumer")
                        .WithBufferSize(10)
                        .WithWorkersCount(20)
                        .WithAutoOffsetReset(AutoOffsetReset.Latest)
                        .AddMiddlewares(
                            middlewares => middlewares
                                .AddSerializer<ProtobufNetSerializer>()
                                .RetryDurable(
                                    configure => configure
                                        .Handle<RetryDurableTestException>()
                                        .WithMessageType(typeof(RetryDurableTestMessage))
                                        .WithSqlServerDataProvider(
                                            sqlServerConnectionString,
                                            sqlServerDatabaseName)
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
                                                .WithRetryTopicName("sample-kafka-flow-retry-durable-sqlserver-topic-retry")
                                                .WithRetryConsumerBufferSize(4)
                                                .WithRetryConsumerWorkersCount(2)
                                                .WithRetryConsumerStrategy(RetryConsumerStrategy.LatestConsumption)
                                                .WithRetryTypedHandlers(
                                                    handlers => handlers
                                                        .WithHandlerLifetime(InstanceLifetime.Transient)
                                                        .AddHandler<RetryDurableTestHandler>()
                                                )
                                                .Enabled(true)
                                        )
                                        .WithQueuePollingJobConfiguration(
                                            configure => configure
                                                .WithId("retry-durable-sqlserver-polling-id")
                                                .WithCronExpression("0 0/1 * 1/1 * ? *")
                                                .WithExpirationIntervalFactor(1)
                                                .WithFetchSize(10)
                                                .Enabled(true)
                                        ))
                                .AddTypedHandlers(
                                    handlers => handlers
                                        .WithHandlerLifetime(InstanceLifetime.Transient)
                                        .AddHandler<RetryDurableTestHandler>())
                        )
                );

            return cluster;
        }

        internal static IClusterConfigurationBuilder SetupRetryForever(this IClusterConfigurationBuilder cluster)
        {
            cluster
                .AddProducer(
                    "kafka-flow-retry-forever-producer",
                    producer => producer
                        .DefaultTopic("sample-kafka-flow-retry-forever-topic")
                        .WithCompression(Confluent.Kafka.CompressionType.Gzip)
                        .AddMiddlewares(
                            middlewares => middlewares
                                .AddSerializer<ProtobufNetSerializer>()
                        )
                        .WithAcks(Acks.All)
                )
                .AddConsumer(
                    consumer => consumer
                        .Topic("sample-kafka-flow-retry-forever-topic")
                        .WithGroupId("sample-consumer-kafka-flow-retry-forever")
                        .WithName("kafka-flow-retry-forever-consumer")
                        .WithBufferSize(10)
                        .WithWorkersCount(20)
                        .WithAutoOffsetReset(AutoOffsetReset.Latest)
                        .AddMiddlewares(
                            middlewares => middlewares
                                .AddSerializer<ProtobufNetSerializer>()
                                .RetryForever(
                                    (configure) => configure
                                        .Handle<RetryForeverTestException>()
                                        .WithTimeBetweenTriesPlan(
                                            TimeSpan.FromMilliseconds(500),
                                            TimeSpan.FromMilliseconds(1000))
                                )
                                .AddTypedHandlers(
                                    handlers => handlers
                                        .WithHandlerLifetime(InstanceLifetime.Transient)
                                        .AddHandler<RetryForeverTestHandler>())
                        )
                );

            return cluster;
        }

        internal static IClusterConfigurationBuilder SetupRetrySimple(this IClusterConfigurationBuilder cluster)
        {
            cluster
                .AddProducer(
                    "kafka-flow-retry-simple-producer",
                    producer => producer
                        .DefaultTopic("sample-kafka-flow-retry-simple-topic")
                        .WithCompression(Confluent.Kafka.CompressionType.Gzip)
                        .AddMiddlewares(
                            middlewares => middlewares
                                .AddSerializer<ProtobufNetSerializer>()
                        )
                        .WithAcks(Acks.All)
                )
                .AddConsumer(
                    consumer => consumer
                        .Topic("sample-kafka-flow-retry-simple-topic")
                        .WithGroupId("sample-consumer-kafka-flow-retry-simple")
                        .WithName("kafka-flow-retry-simple-consumer")
                        .WithBufferSize(10)
                        .WithWorkersCount(20)
                        .WithAutoOffsetReset(AutoOffsetReset.Latest)
                        .AddMiddlewares(
                            middlewares => middlewares
                                .AddSerializer<ProtobufNetSerializer>()
                                .RetrySimple(
                                    (configure) => configure
                                        .Handle<RetrySimpleTestException>()
                                        .TryTimes(2)
                                        .WithTimeBetweenTriesPlan((retryCount) =>
                                        {
                                            var plan = new[]
                                            {
                                            TimeSpan.FromMilliseconds(1500),
                                            TimeSpan.FromMilliseconds(2000),
                                            TimeSpan.FromMilliseconds(2000)
                                            };

                                            return plan[retryCount];
                                        })
                                        .ShouldPauseConsumer(false)
                                )
                        .AddTypedHandlers(
                            handlers => handlers
                                .WithHandlerLifetime(InstanceLifetime.Transient)
                                .AddHandler<RetrySimpleTestHandler>())
                        )
                );

            return cluster;
        }
    }
}