namespace KafkaFlow.Retry.IntegrationTests.Core.Bootstrappers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
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
        internal static async Task RecreateKafkaTopicsAsync(string kafkaBrokers, string[] topics)
        {
            using (var adminClient = new Confluent.Kafka.AdminClientBuilder(new Confluent.Kafka.AdminClientConfig { BootstrapServers = kafkaBrokers }).Build())
            {
                foreach (var topic in topics)
                {
                    var topicMetadata = adminClient.GetMetadata(topic, TimeSpan.FromSeconds(20));
                    if (topicMetadata.Topics.First().Partitions.Count > 0)
                    {
                        try
                        {
                            var deleteTopicRecords = new List<Confluent.Kafka.TopicPartitionOffset>();
                            for (int i = 0; i < topicMetadata.Topics.First().Partitions.Count; i++)
                            {
                                deleteTopicRecords.Add(new Confluent.Kafka.TopicPartitionOffset(topic, i, Confluent.Kafka.Offset.End));
                            }
                            await adminClient.DeleteRecordsAsync(deleteTopicRecords).ConfigureAwait(false);
                        }
                        catch (Confluent.Kafka.Admin.DeleteRecordsException e)
                        {
                            Debug.WriteLine($"An error occured deleting topic records: {e.Results[0].Error.Reason}");
                        }
                    }
                    else
                    {
                        try
                        {
                            await adminClient
                                .CreatePartitionsAsync(
                                    new List<Confluent.Kafka.Admin.PartitionsSpecification>
                                    {
                                        new Confluent.Kafka.Admin.PartitionsSpecification
                                        {
                                            Topic = topic,
                                            IncreaseTo = 6
                                        }
                                    })
                                .ConfigureAwait(false);
                        }
                        catch (Confluent.Kafka.Admin.CreateTopicsException e)
                        {
                            if (e.Results[0].Error.Code != Confluent.Kafka.ErrorCode.UnknownTopicOrPart)
                            {
                                Debug.WriteLine($"An error occured creating a topic: {e.Results[0].Error.Reason}");
                            }
                            else
                            {
                                Debug.WriteLine("Topic does not exists");
                            }
                        }
                    }
                }
            }
        }

        internal static IClusterConfigurationBuilder SetupEmptyRetryDurableGuaranteeOrderedConsumptionMongoDbCluster(
            this IClusterConfigurationBuilder cluster,
            string mongoDbConnectionString,
            string mongoDbDatabaseName,
            string mongoDbRetryQueueCollectionName,
            string mongoDbRetryQueueItemCollectionName)
        {
            cluster
                .AddProducer<EmptyRetryDurableGuaranteeOrderedConsumptionMongoDbProducer>(
                    producer => producer
                        .DefaultTopic("test-kafka-flow-retry-empty-retry-durable-guarantee-ordered-consumption-mongo-db")
                        .WithCompression(Confluent.Kafka.CompressionType.Gzip)
                        .AddMiddlewares(
                            middlewares => middlewares
                                .AddSingleTypeSerializer<RetryDurableTestMessage, NewtonsoftJsonSerializer>()))
                .AddConsumer(
                    consumer => consumer
                        .Topic("test-kafka-flow-retry-empty-retry-durable-guarantee-ordered-consumption-mongo-db")
                        .WithGroupId("test-consumer-kafka-flow-retry-empty-retry-durable-guarantee-ordered-consumption-mongo-db")
                        .WithBufferSize(100)
                        .WithWorkersCount(10)
                        .WithAutoOffsetReset(AutoOffsetReset.Latest)
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
                                                .WithRetryTopicName("test-kafka-flow-retry-empty-retry-durable-guarantee-ordered-consumption-mongo-db-retry")
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
                                                .WithId("custom_search_key_empty_durable_guarantee_ordered_consumption_mongo_db")
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

        internal static IClusterConfigurationBuilder SetupEmptyRetryDurableGuaranteeOrderedConsumptionSqlServerCluster(
            this IClusterConfigurationBuilder cluster,
            string sqlServerConnectionString,
            string sqlServerDatabaseName)
        {
            cluster
                .AddProducer<EmptyRetryDurableGuaranteeOrderedConsumptionSqlServerProducer>(
                    producer => producer
                        .DefaultTopic("test-kafka-flow-retry-empty-retry-durable-guarantee-ordered-consumption-sql-server")
                        .WithCompression(Confluent.Kafka.CompressionType.Gzip)
                        .AddMiddlewares(
                            middlewares => middlewares
                                .AddSingleTypeSerializer<RetryDurableTestMessage, NewtonsoftJsonSerializer>()))
                .AddConsumer(
                    consumer => consumer
                        .Topic("test-kafka-flow-retry-empty-retry-durable-guarantee-ordered-consumption-sql-server")
                        .WithGroupId("test-consumer-kafka-flow-retry-empty-retry-durable-guarantee-ordered-consumption-sql-server")
                        .WithBufferSize(100)
                        .WithWorkersCount(10)
                        .WithAutoOffsetReset(AutoOffsetReset.Latest)
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
                                                .WithRetryTopicName("test-kafka-flow-retry-empty-retry-durable-guarantee-ordered-consumption-sql-server-retry")
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
                                                .WithId("custom_search_key_empty_durable_guarantee_ordered_consumption_sql_server")
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

        internal static IClusterConfigurationBuilder SetupEmptyRetryDurableLatestConsumptionMongoDbCluster(
            this IClusterConfigurationBuilder cluster,
            string mongoDbConnectionString,
            string mongoDbDatabaseName,
            string mongoDbRetryQueueCollectionName,
            string mongoDbRetryQueueItemCollectionName)
        {
            cluster
                .AddProducer<EmptyRetryDurableLatestConsumptionMongoDbProducer>(
                    producer => producer
                        .DefaultTopic("test-kafka-flow-retry-empty-retry-durable-latest-consumption-mongo-db")
                        .WithCompression(Confluent.Kafka.CompressionType.Gzip)
                        .AddMiddlewares(
                            middlewares => middlewares
                                .AddSingleTypeSerializer<RetryDurableTestMessage, NewtonsoftJsonSerializer>()))
                .AddConsumer(
                    consumer => consumer
                        .Topic("test-kafka-flow-retry-empty-retry-durable-latest-consumption-mongo-db")
                        .WithGroupId("test-consumer-kafka-flow-retry-empty-retry-durable-latest-consumption-mongo-db")
                        .WithBufferSize(100)
                        .WithWorkersCount(10)
                        .WithAutoOffsetReset(AutoOffsetReset.Latest)
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
                                                .WithRetryTopicName("test-kafka-flow-retry-empty-retry-durable-latest-consumption-mongo-db-retry")
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
                                                .WithId("custom_search_key_empty_durable_latest_consumption_mongo_db")
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

        internal static IClusterConfigurationBuilder SetupEmptyRetryDurableLatestConsumptionSqlServerCluster(
            this IClusterConfigurationBuilder cluster,
            string sqlServerConnectionString,
            string sqlServerDatabaseName)
        {
            cluster
                .AddProducer<EmptyRetryDurableLatestConsumptionSqlServerProducer>(
                    producer => producer
                        .DefaultTopic("test-kafka-flow-retry-empty-retry-durable-latest-consumption-sql-server")
                        .WithCompression(Confluent.Kafka.CompressionType.Gzip)
                        .AddMiddlewares(
                            middlewares => middlewares
                                .AddSingleTypeSerializer<RetryDurableTestMessage, NewtonsoftJsonSerializer>()))
                .AddConsumer(
                    consumer => consumer
                        .Topic("test-kafka-flow-retry-empty-retry-durable-latest-consumption-sql-server")
                        .WithGroupId("test-consumer-kafka-flow-retry-empty-retry-durable-latest-consumption-sql-server")
                        .WithBufferSize(100)
                        .WithWorkersCount(10)
                        .WithAutoOffsetReset(AutoOffsetReset.Latest)
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
                                                .WithRetryTopicName("test-kafka-flow-retry-empty-retry-durable-latest-consumption-sql-server-retry")
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
                                                .WithId("custom_search_key_empty_durable_latest_consumption_sql_server")
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

        internal static IClusterConfigurationBuilder SetupNullRetryDurableGuaranteeOrderedConsumptionMongoDbCluster(
                                    this IClusterConfigurationBuilder cluster,
            string mongoDbConnectionString,
            string mongoDbDatabaseName,
            string mongoDbRetryQueueCollectionName,
            string mongoDbRetryQueueItemCollectionName)
        {
            cluster
                .AddProducer<NullRetryDurableGuaranteeOrderedConsumptionMongoDbProducer>(
                    producer => producer
                        .DefaultTopic("test-kafka-flow-retry-null-retry-durable-guarantee-ordered-consumption-mongo-db")
                        .WithCompression(Confluent.Kafka.CompressionType.Gzip)
                        .AddMiddlewares(
                            middlewares => middlewares
                                .AddSingleTypeSerializer<RetryDurableTestMessage, NewtonsoftJsonSerializer>()))
                .AddConsumer(
                    consumer => consumer
                        .Topic("test-kafka-flow-retry-null-retry-durable-guarantee-ordered-consumption-mongo-db")
                        .WithGroupId("test-consumer-kafka-flow-retry-null-retry-durable-guarantee-ordered-consumption-mongo-db")
                        .WithBufferSize(100)
                        .WithWorkersCount(10)
                        .WithAutoOffsetReset(AutoOffsetReset.Latest)
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
                                                .WithRetryTopicName("test-kafka-flow-retry-null-retry-durable-guarantee-ordered-consumption-mongo-db-retry")
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
                                                .WithId("custom_search_key_null_durable_guarantee_ordered_consumption_mongo_db")
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

        internal static IClusterConfigurationBuilder SetupNullRetryDurableGuaranteeOrderedConsumptionSqlServerCluster(
            this IClusterConfigurationBuilder cluster,
            string sqlServerConnectionString,
            string sqlServerDatabaseName)
        {
            cluster
                .AddProducer<NullRetryDurableGuaranteeOrderedConsumptionSqlServerProducer>(
                    producer => producer
                        .DefaultTopic("test-kafka-flow-retry-null-retry-durable-guarantee-ordered-consumption-sql-server")
                        .WithCompression(Confluent.Kafka.CompressionType.Gzip)
                        .AddMiddlewares(
                            middlewares => middlewares
                                .AddSingleTypeSerializer<RetryDurableTestMessage, NewtonsoftJsonSerializer>()))
                .AddConsumer(
                    consumer => consumer
                        .Topic("test-kafka-flow-retry-null-retry-durable-guarantee-ordered-consumption-sql-server")
                        .WithGroupId("test-consumer-kafka-flow-retry-null-retry-durable-guarantee-ordered-consumption-sql-server")
                        .WithBufferSize(100)
                        .WithWorkersCount(10)
                        .WithAutoOffsetReset(AutoOffsetReset.Latest)
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
                                                .WithRetryTopicName("test-kafka-flow-retry-null-retry-durable-guarantee-ordered-consumption-sql-server-retry")
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
                                                .WithId("custom_search_key_null_durable_guarantee_ordered_consumption_sql_server")
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

        internal static IClusterConfigurationBuilder SetupNullRetryDurableLatestConsumptionMongoDbCluster(
            this IClusterConfigurationBuilder cluster,
            string mongoDbConnectionString,
            string mongoDbDatabaseName,
            string mongoDbRetryQueueCollectionName,
            string mongoDbRetryQueueItemCollectionName)
        {
            cluster
                .AddProducer<NullRetryDurableLatestConsumptionMongoDbProducer>(
                    producer => producer
                        .DefaultTopic("test-kafka-flow-retry-null-retry-durable-latest-consumption-mongo-db")
                        .WithCompression(Confluent.Kafka.CompressionType.Gzip)
                        .AddMiddlewares(
                            middlewares => middlewares
                                .AddSingleTypeSerializer<RetryDurableTestMessage, NewtonsoftJsonSerializer>()))
                .AddConsumer(
                    consumer => consumer
                        .Topic("test-kafka-flow-retry-null-retry-durable-latest-consumption-mongo-db")
                        .WithGroupId("test-consumer-kafka-flow-retry-null-retry-durable-latest-consumption-mongo-db")
                        .WithBufferSize(100)
                        .WithWorkersCount(10)
                        .WithAutoOffsetReset(AutoOffsetReset.Latest)
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
                                                .WithRetryTopicName("test-kafka-flow-retry-null-retry-durable-latest-consumption-mongo-db-retry")
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
                                                .WithId("custom_search_key_null_durable_latest_consumption_mongo_db")
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

        internal static IClusterConfigurationBuilder SetupNullRetryDurableLatestConsumptionSqlServerCluster(
            this IClusterConfigurationBuilder cluster,
            string sqlServerConnectionString,
            string sqlServerDatabaseName)
        {
            cluster
                .AddProducer<NullRetryDurableLatestConsumptionSqlServerProducer>(
                    producer => producer
                        .DefaultTopic("test-kafka-flow-retry-null-retry-durable-latest-consumption-sql-server")
                        .WithCompression(Confluent.Kafka.CompressionType.Gzip)
                        .AddMiddlewares(
                            middlewares => middlewares
                                .AddSingleTypeSerializer<RetryDurableTestMessage, NewtonsoftJsonSerializer>()))
                .AddConsumer(
                    consumer => consumer
                        .Topic("test-kafka-flow-retry-null-retry-durable-latest-consumption-sql-server")
                        .WithGroupId("test-consumer-kafka-flow-retry-null-retry-durable-latest-consumption-sql-server")
                        .WithBufferSize(100)
                        .WithWorkersCount(10)
                        .WithAutoOffsetReset(AutoOffsetReset.Latest)
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
                                                .WithRetryTopicName("test-kafka-flow-retry-null-retry-durable-latest-consumption-sql-server-retry")
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
                                                .WithId("custom_search_key_null_durable_latest_consumption_sql_server")
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
                        .WithCompression(Confluent.Kafka.CompressionType.Gzip)
                        .AddMiddlewares(
                            middlewares => middlewares
                                .AddSingleTypeSerializer<RetryDurableTestMessage, NewtonsoftJsonSerializer>()))
                .AddConsumer(
                    consumer => consumer
                        .Topic("test-kafka-flow-retry-retry-durable-guarantee-ordered-consumption-mongo-db")
                        .WithGroupId("test-consumer-kafka-flow-retry-retry-durable-guarantee-ordered-consumption-mongo-db")
                        .WithBufferSize(100)
                        .WithWorkersCount(10)
                        .WithAutoOffsetReset(AutoOffsetReset.Latest)
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
                        .WithAutoOffsetReset(AutoOffsetReset.Latest)
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
                        .WithAutoOffsetReset(AutoOffsetReset.Latest)
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
                        .WithAutoOffsetReset(AutoOffsetReset.Latest)
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
                        .WithAutoOffsetReset(AutoOffsetReset.Latest)
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
                        .WithAutoOffsetReset(AutoOffsetReset.Latest)
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