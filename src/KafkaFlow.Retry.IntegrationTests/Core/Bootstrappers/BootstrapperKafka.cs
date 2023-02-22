using KafkaFlow.Retry.Postgres;

namespace KafkaFlow.Retry.IntegrationTests.Core.Bootstrappers
{
    using System;
    using Confluent.Kafka;
    using KafkaFlow.Configuration;
    using KafkaFlow.Retry.IntegrationTests.Core.Exceptions;
    using KafkaFlow.Retry.IntegrationTests.Core.Handlers;
    using KafkaFlow.Retry.IntegrationTests.Core.Messages;
    using KafkaFlow.Retry.IntegrationTests.Core.Producers;
    using KafkaFlow.Retry.MongoDb;
    using KafkaFlow.Retry.SqlServer;
    using KafkaFlow.Serializer;
    using KafkaFlow.TypedHandler;
    using Newtonsoft.Json;

    internal static class BootstrapperKafka
    {
        private const int NumberOfPartitions = 6;
        private const int ReplicationFactor = 1;

        private static readonly string[] TestTopics = new[]
        {
             "test-kafka-flow-retry-retry-simple",
             "test-kafka-flow-retry-retry-forever",
             "test-kafka-flow-retry-retry-durable-guarantee-ordered-consumption-mongo-db",
             "test-kafka-flow-retry-retry-durable-guarantee-ordered-consumption-mongo-db-retry",
             "test-kafka-flow-retry-retry-durable-guarantee-ordered-consumption-sql-server",
             "test-kafka-flow-retry-retry-durable-guarantee-ordered-consumption-sql-server-retry",
             "test-kafka-flow-retry-retry-durable-guarantee-ordered-consumption-postgres",
             "test-kafka-flow-retry-retry-durable-guarantee-ordered-consumption-postgres-retry",
             "test-kafka-flow-retry-retry-durable-latest-consumption-mongo-db",
             "test-kafka-flow-retry-retry-durable-latest-consumption-mongo-db-retry",
             "test-kafka-flow-retry-retry-durable-latest-consumption-sql-server",
             "test-kafka-flow-retry-retry-durable-latest-consumption-sql-server-retry",
             "test-kafka-flow-retry-retry-durable-latest-consumption-postgres",
             "test-kafka-flow-retry-retry-durable-latest-consumption-postgres-retry"
        };

        internal static IClusterConfigurationBuilder CreatAllTestTopicsIfNotExist(this IClusterConfigurationBuilder cluster)
        {
            foreach (var topic in TestTopics)
            {
                cluster.CreateTopicIfNotExists(topic, NumberOfPartitions, ReplicationFactor);
            }

            return cluster;
        }

        internal static IClusterConfigurationBuilder SetupRetryDurableGuaranteeOrderedConsumptionMongoDbCluster(
            this IClusterConfigurationBuilder cluster,
            string mongoDbConnectionString,
            string mongoDbDatabaseName,
            string mongoDbRetryQueueCollectionName,
            string mongoDbRetryQueueItemCollectionName)
        {
            cluster
                .AddProducer<RetryDurableGuaranteeOrderedConsumptionMongoDbProducer>(
                    producer => producer
                        .DefaultTopic("test-kafka-flow-retry-retry-durable-guarantee-ordered-consumption-mongo-db")
                        .WithCompression(CompressionType.Gzip)
                        .AddMiddlewares(
                            middlewares => middlewares
                                .AddSingleTypeSerializer<RetryDurableTestMessage, NewtonsoftJsonSerializer>()))
                .AddConsumer(
                    consumer => consumer
                        .Topic("test-kafka-flow-retry-retry-durable-guarantee-ordered-consumption-mongo-db")
                        .WithGroupId("test-consumer-kafka-flow-retry-retry-durable-guarantee-ordered-consumption-mongo-db")
                        .WithBufferSize(100)
                        .WithWorkersCount(10)
                        .WithAutoOffsetReset(KafkaFlow.AutoOffsetReset.Latest)
                        .AddMiddlewares(
                            middlewares => middlewares
                                .AddSingleTypeSerializer<NewtonsoftJsonSerializer>(typeof(RetryDurableTestMessage))
                                .RetryDurable(
                                    (configure) => configure
                                        .Handle<RetryDurableTestException>()
                                        .WithMessageType(typeof(RetryDurableTestMessage))
                                        .WithMessageSerializeSettings(
                                            new JsonSerializerSettings
                                            {
                                                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                                                TypeNameHandling = TypeNameHandling.Auto
                                            })
                                        .WithEmbeddedRetryCluster(
                                            cluster,
                                            configure => configure
                                                .Enabled(true)
                                                .WithRetryTopicName("test-kafka-flow-retry-retry-durable-guarantee-ordered-consumption-mongo-db-retry")
                                                .WithRetryConsumerBufferSize(100)
                                                .WithRetryConsumerWorkersCount(10)
                                                .WithRetryConsumerStrategy(RetryConsumerStrategy.GuaranteeOrderedConsumption)
                                                .WithRetryTypedHandlers(
                                                    handlers => handlers
                                                        .WithHandlerLifetime(InstanceLifetime.Transient)
                                                        .AddHandler<RetryDurableTestMessageHandler>()))
                                        .WithQueuePollingJobConfiguration(
                                            configure => configure
                                                .Enabled(true)
                                                .WithId("custom_search_key_durable_guarantee_ordered_consumption_mongo_db")
                                                .WithCronExpression("0/30 * * ? * * *")
                                                .WithExpirationIntervalFactor(1)
                                                .WithFetchSize(256))
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
                                                .ShouldPauseConsumer(false)))
                                .AddTypedHandlers(
                                    handlers =>
                                        handlers
                                            .WithHandlerLifetime(InstanceLifetime.Singleton)
                                            .AddHandler<RetryDurableTestMessageHandler>())));
            return cluster;
        }

        internal static IClusterConfigurationBuilder SetupRetryDurableGuaranteeOrderedConsumptionSqlServerCluster(
            this IClusterConfigurationBuilder cluster,
            string sqlServerConnectionString,
            string sqlServerDatabaseName)
        {
            cluster
                .AddProducer<RetryDurableGuaranteeOrderedConsumptionSqlServerProducer>(
                    producer => producer
                        .DefaultTopic("test-kafka-flow-retry-retry-durable-guarantee-ordered-consumption-sql-server")
                        .WithCompression(Confluent.Kafka.CompressionType.Gzip)
                        .AddMiddlewares(
                            middlewares => middlewares
                                .AddSingleTypeSerializer<RetryDurableTestMessage, NewtonsoftJsonSerializer>()))
                .AddConsumer(
                    consumer => consumer
                        .Topic("test-kafka-flow-retry-retry-durable-guarantee-ordered-consumption-sql-server")
                        .WithGroupId("test-consumer-kafka-flow-retry-retry-durable-guarantee-ordered-consumption-sql-server")
                        .WithBufferSize(100)
                        .WithWorkersCount(10)
                        .WithAutoOffsetReset(KafkaFlow.AutoOffsetReset.Latest)
                        .AddMiddlewares(
                            middlewares => middlewares
                                .AddSingleTypeSerializer<NewtonsoftJsonSerializer>(typeof(RetryDurableTestMessage))
                                .RetryDurable(
                                    (configure) => configure
                                        .Handle<RetryDurableTestException>()
                                        .WithMessageType(typeof(RetryDurableTestMessage))
                                        .WithMessageSerializeSettings(
                                            new JsonSerializerSettings
                                            {
                                                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                                                TypeNameHandling = TypeNameHandling.Auto
                                            })
                                        .WithEmbeddedRetryCluster(
                                            cluster,
                                            configure => configure
                                                .Enabled(true)
                                                .WithRetryTopicName("test-kafka-flow-retry-retry-durable-guarantee-ordered-consumption-sql-server-retry")
                                                .WithRetryConsumerBufferSize(100)
                                                .WithRetryConsumerWorkersCount(10)
                                                .WithRetryConsumerStrategy(RetryConsumerStrategy.GuaranteeOrderedConsumption)
                                                .WithRetryTypedHandlers(
                                                    handlers => handlers
                                                        .WithHandlerLifetime(InstanceLifetime.Transient)
                                                        .AddHandler<RetryDurableTestMessageHandler>()))
                                        .WithQueuePollingJobConfiguration(
                                            configure => configure
                                                .Enabled(true)
                                                .WithId("custom_search_key_durable_guarantee_ordered_consumption_sql_server")
                                                .WithCronExpression("0/30 * * ? * * *")
                                                .WithExpirationIntervalFactor(1)
                                                .WithFetchSize(256))
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
                                                .ShouldPauseConsumer(false)))
                                .AddTypedHandlers(
                                    handlers =>
                                        handlers
                                            .WithHandlerLifetime(InstanceLifetime.Singleton)
                                            .AddHandler<RetryDurableTestMessageHandler>())));
            return cluster;
        }

        internal static IClusterConfigurationBuilder SetupRetryDurableGuaranteeOrderedConsumptionPostgresCluster(
            this IClusterConfigurationBuilder cluster,
            string postgresConnectionString,
            string postgresDatabaseName)
        {
            cluster
                .AddProducer<RetryDurableGuaranteeOrderedConsumptionPostgresProducer>(
                    producer => producer
                        .DefaultTopic("test-kafka-flow-retry-retry-durable-guarantee-ordered-consumption-postgres")
                        .WithCompression(Confluent.Kafka.CompressionType.Gzip)
                        .AddMiddlewares(
                            middlewares => middlewares
                                .AddSingleTypeSerializer<RetryDurableTestMessage, NewtonsoftJsonSerializer>()))
                .AddConsumer(
                    consumer => consumer
                        .Topic("test-kafka-flow-retry-retry-durable-guarantee-ordered-consumption-postgres")
                        .WithGroupId("test-consumer-kafka-flow-retry-retry-durable-guarantee-ordered-consumption-postgres")
                        .WithBufferSize(100)
                        .WithWorkersCount(10)
                        .WithAutoOffsetReset(KafkaFlow.AutoOffsetReset.Latest)
                        .AddMiddlewares(
                            middlewares => middlewares
                                .AddSingleTypeSerializer<NewtonsoftJsonSerializer>(typeof(RetryDurableTestMessage))
                                .RetryDurable(
                                    (configure) => configure
                                        .Handle<RetryDurableTestException>()
                                        .WithMessageType(typeof(RetryDurableTestMessage))
                                        .WithMessageSerializeSettings(
                                            new JsonSerializerSettings
                                            {
                                                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                                                TypeNameHandling = TypeNameHandling.Auto
                                            })
                                        .WithEmbeddedRetryCluster(
                                            cluster,
                                            configure => configure
                                                .Enabled(true)
                                                .WithRetryTopicName("test-kafka-flow-retry-retry-durable-guarantee-ordered-consumption-postgres-retry")
                                                .WithRetryConsumerBufferSize(100)
                                                .WithRetryConsumerWorkersCount(10)
                                                .WithRetryConsumerStrategy(RetryConsumerStrategy.GuaranteeOrderedConsumption)
                                                .WithRetryTypedHandlers(
                                                    handlers => handlers
                                                        .WithHandlerLifetime(InstanceLifetime.Transient)
                                                        .AddHandler<RetryDurableTestMessageHandler>()))
                                        .WithQueuePollingJobConfiguration(
                                            configure => configure
                                                .Enabled(true)
                                                .WithId("custom_search_key_durable_guarantee_ordered_consumption_postgres")
                                                .WithCronExpression("0/30 * * ? * * *")
                                                .WithExpirationIntervalFactor(1)
                                                .WithFetchSize(256))
                                        .WithPostgresDataProvider(
                                            postgresConnectionString,
                                            postgresDatabaseName)
                                        .WithRetryPlanBeforeRetryDurable(
                                            configure => configure
                                                .TryTimes(3)
                                                .WithTimeBetweenTriesPlan(
                                                    TimeSpan.FromMilliseconds(250),
                                                    TimeSpan.FromMilliseconds(500),
                                                    TimeSpan.FromMilliseconds(1000))
                                                .ShouldPauseConsumer(false)))
                                .AddTypedHandlers(
                                    handlers =>
                                        handlers
                                            .WithHandlerLifetime(InstanceLifetime.Singleton)
                                            .AddHandler<RetryDurableTestMessageHandler>())));
            return cluster;
        }

        internal static IClusterConfigurationBuilder SetupRetryDurableLatestConsumptionMongoDbCluster(
            this IClusterConfigurationBuilder cluster,
            string mongoDbConnectionString,
            string mongoDbDatabaseName,
            string mongoDbRetryQueueCollectionName,
            string mongoDbRetryQueueItemCollectionName)
        {
            cluster
                .AddProducer<RetryDurableLatestConsumptionMongoDbProducer>(
                    producer => producer
                        .DefaultTopic("test-kafka-flow-retry-retry-durable-latest-consumption-mongo-db")
                        .WithCompression(Confluent.Kafka.CompressionType.Gzip)
                        .AddMiddlewares(
                            middlewares => middlewares
                                .AddSingleTypeSerializer<RetryDurableTestMessage, NewtonsoftJsonSerializer>()))
                .AddConsumer(
                    consumer => consumer
                        .Topic("test-kafka-flow-retry-retry-durable-latest-consumption-mongo-db")
                        .WithGroupId("test-consumer-kafka-flow-retry-retry-durable-latest-consumption-mongo-db")
                        .WithBufferSize(100)
                        .WithWorkersCount(10)
                        .WithAutoOffsetReset(KafkaFlow.AutoOffsetReset.Latest)
                        .AddMiddlewares(
                            middlewares => middlewares
                                .AddSingleTypeSerializer<NewtonsoftJsonSerializer>(typeof(RetryDurableTestMessage))
                                .RetryDurable(
                                    (configure) => configure
                                        .Handle<RetryDurableTestException>()
                                        .WithMessageType(typeof(RetryDurableTestMessage))
                                        .WithMessageSerializeSettings(
                                            new JsonSerializerSettings
                                            {
                                                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                                                TypeNameHandling = TypeNameHandling.Auto
                                            })
                                        .WithEmbeddedRetryCluster(
                                            cluster,
                                            configure => configure
                                                .Enabled(true)
                                                .WithRetryTopicName("test-kafka-flow-retry-retry-durable-latest-consumption-mongo-db-retry")
                                                .WithRetryConsumerBufferSize(100)
                                                .WithRetryConsumerWorkersCount(10)
                                                .WithRetryConsumerStrategy(RetryConsumerStrategy.LatestConsumption)
                                                .WithRetryTypedHandlers(
                                                    handlers => handlers
                                                        .WithHandlerLifetime(InstanceLifetime.Transient)
                                                        .AddHandler<RetryDurableTestMessageHandler>()))
                                        .WithQueuePollingJobConfiguration(
                                            configure => configure
                                                .Enabled(true)
                                                .WithId("custom_search_key_durable_latest_consumption_mongo_db")
                                                .WithCronExpression("0/30 * * ? * * *")
                                                .WithExpirationIntervalFactor(1)
                                                .WithFetchSize(256))
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
                                                .ShouldPauseConsumer(false)))
                                .AddTypedHandlers(
                                    handlers =>
                                        handlers
                                            .WithHandlerLifetime(InstanceLifetime.Singleton)
                                            .AddHandler<RetryDurableTestMessageHandler>())));
            return cluster;
        }

        internal static IClusterConfigurationBuilder SetupRetryDurableLatestConsumptionSqlServerCluster(
            this IClusterConfigurationBuilder cluster,
            string sqlServerConnectionString,
            string sqlServerDatabaseName)
        {
            cluster
                .AddProducer<RetryDurableLatestConsumptionSqlServerProducer>(
                    producer => producer
                        .DefaultTopic("test-kafka-flow-retry-retry-durable-latest-consumption-sql-server")
                        .WithCompression(Confluent.Kafka.CompressionType.Gzip)
                        .AddMiddlewares(
                            middlewares => middlewares
                                .AddSingleTypeSerializer<RetryDurableTestMessage, NewtonsoftJsonSerializer>()))
                .AddConsumer(
                    consumer => consumer
                        .Topic("test-kafka-flow-retry-retry-durable-latest-consumption-sql-server")
                        .WithGroupId("test-consumer-kafka-flow-retry-retry-durable-latest-consumption-sql-server")
                        .WithBufferSize(100)
                        .WithWorkersCount(10)
                        .WithAutoOffsetReset((KafkaFlow.AutoOffsetReset)AutoOffsetReset.Latest)
                        .AddMiddlewares(
                            middlewares => middlewares
                                .AddSingleTypeSerializer<NewtonsoftJsonSerializer>(typeof(RetryDurableTestMessage))
                                .RetryDurable(
                                    (configure) => configure
                                        .Handle<RetryDurableTestException>()
                                        .WithMessageType(typeof(RetryDurableTestMessage))
                                        .WithMessageSerializeSettings(
                                            new JsonSerializerSettings
                                            {
                                                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                                                TypeNameHandling = TypeNameHandling.Auto
                                            })
                                        .WithEmbeddedRetryCluster(
                                            cluster,
                                            configure => configure
                                                .Enabled(true)
                                                .WithRetryTopicName("test-kafka-flow-retry-retry-durable-latest-consumption-sql-server-retry")
                                                .WithRetryConsumerBufferSize(100)
                                                .WithRetryConsumerWorkersCount(10)
                                                .WithRetryConsumerStrategy(RetryConsumerStrategy.LatestConsumption)
                                                .WithRetryTypedHandlers(
                                                    handlers => handlers
                                                        .WithHandlerLifetime(InstanceLifetime.Transient)
                                                        .AddHandler<RetryDurableTestMessageHandler>()))
                                        .WithQueuePollingJobConfiguration(
                                            configure => configure
                                                .Enabled(true)
                                                .WithId("custom_search_key_durable_latest_consumption_sql_server")
                                                .WithCronExpression("0/30 * * ? * * *")
                                                .WithExpirationIntervalFactor(1)
                                                .WithFetchSize(256))
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
                                                .ShouldPauseConsumer(false)))
                                .AddTypedHandlers(
                                    handlers =>
                                        handlers
                                            .WithHandlerLifetime(InstanceLifetime.Singleton)
                                            .AddHandler<RetryDurableTestMessageHandler>())));
            return cluster;
        }

        internal static IClusterConfigurationBuilder SetupRetryDurableLatestConsumptionPostgresCluster(
            this IClusterConfigurationBuilder cluster,
            string postgresConnectionString,
            string postgresDatabaseName)
        {
            cluster
                .AddProducer<RetryDurableLatestConsumptionPostgresProducer>(
                    producer => producer
                        .DefaultTopic("test-kafka-flow-retry-retry-durable-latest-consumption-postgres")
                        .WithCompression(Confluent.Kafka.CompressionType.Gzip)
                        .AddMiddlewares(
                            middlewares => middlewares
                                .AddSingleTypeSerializer<RetryDurableTestMessage, NewtonsoftJsonSerializer>()))
                .AddConsumer(
                    consumer => consumer
                        .Topic("test-kafka-flow-retry-retry-durable-latest-consumption-postgres")
                        .WithGroupId("test-consumer-kafka-flow-retry-retry-durable-latest-consumption-postgres")
                        .WithBufferSize(100)
                        .WithWorkersCount(10)
                        .WithAutoOffsetReset((KafkaFlow.AutoOffsetReset)AutoOffsetReset.Latest)
                        .AddMiddlewares(
                            middlewares => middlewares
                                .AddSingleTypeSerializer<NewtonsoftJsonSerializer>(typeof(RetryDurableTestMessage))
                                .RetryDurable(
                                    (configure) => configure
                                        .Handle<RetryDurableTestException>()
                                        .WithMessageType(typeof(RetryDurableTestMessage))
                                        .WithMessageSerializeSettings(
                                            new JsonSerializerSettings
                                            {
                                                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                                                TypeNameHandling = TypeNameHandling.Auto
                                            })
                                        .WithEmbeddedRetryCluster(
                                            cluster,
                                            configure => configure
                                                .Enabled(true)
                                                .WithRetryTopicName("test-kafka-flow-retry-retry-durable-latest-consumption-postgres-retry")
                                                .WithRetryConsumerBufferSize(100)
                                                .WithRetryConsumerWorkersCount(10)
                                                .WithRetryConsumerStrategy(RetryConsumerStrategy.LatestConsumption)
                                                .WithRetryTypedHandlers(
                                                    handlers => handlers
                                                        .WithHandlerLifetime(InstanceLifetime.Transient)
                                                        .AddHandler<RetryDurableTestMessageHandler>()))
                                        .WithQueuePollingJobConfiguration(
                                            configure => configure
                                                .Enabled(true)
                                                .WithId("custom_search_key_durable_latest_consumption_postgres")
                                                .WithCronExpression("0/30 * * ? * * *")
                                                .WithExpirationIntervalFactor(1)
                                                .WithFetchSize(256))
                                        .WithPostgresDataProvider(
                                            postgresConnectionString,
                                            postgresDatabaseName)
                                        .WithRetryPlanBeforeRetryDurable(
                                            configure => configure
                                                .TryTimes(3)
                                                .WithTimeBetweenTriesPlan(
                                                    TimeSpan.FromMilliseconds(250),
                                                    TimeSpan.FromMilliseconds(500),
                                                    TimeSpan.FromMilliseconds(1000))
                                                .ShouldPauseConsumer(false)))
                                .AddTypedHandlers(
                                    handlers =>
                                        handlers
                                            .WithHandlerLifetime(InstanceLifetime.Singleton)
                                            .AddHandler<RetryDurableTestMessageHandler>())));
            return cluster;
        }

        internal static IClusterConfigurationBuilder SetupRetryForeverCluster(this IClusterConfigurationBuilder cluster)
        {
            cluster
                .AddProducer<RetryForeverProducer>(
                    producer => producer
                        .DefaultTopic("test-kafka-flow-retry-retry-forever")
                        .WithCompression(Confluent.Kafka.CompressionType.Gzip)
                        .AddMiddlewares(
                            middlewares => middlewares
                                .AddSingleTypeSerializer<RetryForeverTestMessage, ProtobufNetSerializer>()))
                .AddConsumer(
                    consumer => consumer
                        .Topic("test-kafka-flow-retry-retry-forever")
                        .WithGroupId("test-consumer-kafka-flow-retry-retry-forever")
                        .WithBufferSize(100)
                        .WithWorkersCount(10)
                        .WithAutoOffsetReset(KafkaFlow.AutoOffsetReset.Latest)
                        .AddMiddlewares(
                            middlewares => middlewares
                                .AddSingleTypeSerializer<ProtobufNetSerializer>(typeof(RetryForeverTestMessage))
                                .RetryForever(
                                    (configure) => configure
                                        .Handle<RetryForeverTestException>()
                                        .WithTimeBetweenTriesPlan(TimeSpan.FromMilliseconds(100)))
                                .AddTypedHandlers(
                                    handlers =>
                                        handlers
                                            .WithHandlerLifetime(InstanceLifetime.Singleton)
                                            .AddHandler<RetryForeverTestMessageHandler>())));
            return cluster;
        }

        internal static IClusterConfigurationBuilder SetupRetrySimpleCluster(this IClusterConfigurationBuilder cluster)
        {
            cluster
                .AddProducer<RetrySimpleProducer>(
                    producer => producer
                        .DefaultTopic("test-kafka-flow-retry-retry-simple")
                        .WithCompression(Confluent.Kafka.CompressionType.Gzip)
                        .AddMiddlewares(
                            middlewares => middlewares
                                .AddSingleTypeSerializer<RetrySimpleTestMessage, NewtonsoftJsonSerializer>()))
                .AddConsumer(
                    consumer => consumer
                        .Topic("test-kafka-flow-retry-retry-simple")
                        .WithGroupId("test-consumer-kafka-flow-retry-retry-simple")
                        .WithBufferSize(100)
                        .WithWorkersCount(10)
                        .WithAutoOffsetReset(KafkaFlow.AutoOffsetReset.Latest)
                        .AddMiddlewares(
                            middlewares => middlewares
                                .AddSingleTypeSerializer<NewtonsoftJsonSerializer>(typeof(RetrySimpleTestMessage))
                                .RetrySimple(
                                    (configure) => configure
                                        .Handle<RetrySimpleTestException>()
                                        .TryTimes(3)
                                        .ShouldPauseConsumer(false)
                                        .WithTimeBetweenTriesPlan(
                                            (retryCount) =>
                                            {
                                                var plan = new[]
                                                {
                                                    TimeSpan.FromMilliseconds(100),
                                                    TimeSpan.FromMilliseconds(100),
                                                    TimeSpan.FromMilliseconds(100),
                                                    TimeSpan.FromMilliseconds(100)
                                                };

                                                return plan[retryCount];
                                            }))
                                .AddTypedHandlers(
                                    handlers =>
                                        handlers
                                            .WithHandlerLifetime(InstanceLifetime.Singleton)
                                            .AddHandler<RetrySimpleTestMessageHandler>())));
            return cluster;
        }
    }
}