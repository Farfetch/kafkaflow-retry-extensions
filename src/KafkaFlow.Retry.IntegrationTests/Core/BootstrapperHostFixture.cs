namespace KafkaFlow.Retry.IntegrationTests.Core
{
    using System;
    using System.IO;
    using System.Threading;
    using global::Microsoft.Extensions.Configuration;
    using global::Microsoft.Extensions.DependencyInjection;
    using global::Microsoft.Extensions.Hosting;
    using KafkaFlow.Retry.IntegrationTests.Core.Exceptions;
    using KafkaFlow.Retry.IntegrationTests.Core.Handlers;
    using KafkaFlow.Retry.IntegrationTests.Core.Messages;
    using KafkaFlow.Retry.IntegrationTests.Core.Producers;
    using KafkaFlow.Retry.IntegrationTests.Core.Storages;
    using KafkaFlow.Retry.MongoDb;
    using KafkaFlow.Retry.SqlServer;
    using KafkaFlow.Serializer;
    using KafkaFlow.TypedHandler;
    using Xunit;
    using AutoOffsetReset = KafkaFlow.AutoOffsetReset;

    [CollectionDefinition("BootstrapperHostCollection")]
    public class BootstrapperHosCollectionFixture : ICollectionFixture<BootstrapperHostFixture>
    { }

    public class BootstrapperHostFixture : IDisposable
    {
        private const string RetryForeverTopicName = "test-retry-forever";
        private const string RetrySimpleTopicName = "test-retry-simple";
        private static IKafkaBus kafkaBus;

        public BootstrapperHostFixture()
        {
            var builder = Host
                .CreateDefaultBuilder()
                .ConfigureAppConfiguration(
                    (_, config) =>
                    {
                        config
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile(
                                "conf/appsettings.json",
                                false,
                                true)
                            .AddEnvironmentVariables();
                    })
                .ConfigureServices(SetupServices)
                .UseDefaultServiceProvider(
                    (_, options) =>
                    {
                        options.ValidateScopes = true;
                        options.ValidateOnBuild = true;
                    });

            var host = builder.Build();
            kafkaBus = host.Services.CreateKafkaBus();
            kafkaBus.StartAsync().GetAwaiter().GetResult();

            // Wait partition assignment
            Thread.Sleep(10000);

            ServiceProvider = host.Services;
        }

        public IServiceProvider ServiceProvider { get; private set; }

        public void Dispose()
        {
            kafkaBus.StopAsync().GetAwaiter().GetResult();
        }

        private static void SetupServices(HostBuilderContext context, IServiceCollection services)
        {
            var kafkaBrokers = context.Configuration.GetValue<string>("Kafka:Brokers");
            var mongoDbConnectionString = context.Configuration.GetValue<string>("MongoDbRepository:ConnectionString");
            var mongoDbDatabaseName = context.Configuration.GetValue<string>("MongoDbRepository:DatabaseName");
            var mongoDbRetryQueueCollectionName = context.Configuration.GetValue<string>("MongoDbRepository:RetryQueueCollectionName");
            var mongoDbRetryQueueItemCollectionName = context.Configuration.GetValue<string>("MongoDbRepository:RetryQueueItemCollectionName");
            var sqlServerConnectionString = context.Configuration.GetValue<string>("SqlServerRepository:ConnectionString");
            var sqlServerDatabaseName = context.Configuration.GetValue<string>("SqlServerRepository:DatabaseName");

            services.AddKafka(
                kafka => kafka
                    .UseLogHandler<TraceLogHandler>()
                    .AddCluster(
                        cluster => cluster
                            .WithBrokers(kafkaBrokers.Split(';'))
                            .AddProducer<RetrySimpleProducer>(
                                producer => producer
                                    .DefaultTopic(RetrySimpleTopicName)
                                    .WithCompression(Confluent.Kafka.CompressionType.Gzip)
                                    .AddMiddlewares(
                                        middlewares => middlewares
                                            .AddSingleTypeSerializer<RetrySimpleTestMessage, NewtonsoftJsonSerializer>()))
                            .AddProducer<RetryForeverProducer>(
                                producer => producer
                                    .DefaultTopic(RetryForeverTopicName)
                                    .WithCompression(Confluent.Kafka.CompressionType.Gzip)
                                    .AddMiddlewares(
                                        middlewares => middlewares
                                            .AddSingleTypeSerializer<RetryForeverTestMessage, ProtobufNetSerializer>()))
                            .AddProducer<RetryDurableGuaranteeOrderedConsumptionMongoDbProducer>(
                                producer => producer
                                    .DefaultTopic("test-retry-durable-guarantee-ordered-consumption-mongo-db")
                                    .WithCompression(Confluent.Kafka.CompressionType.Gzip)
                                    .AddMiddlewares(
                                        middlewares => middlewares
                                            .AddSingleTypeSerializer<RetryDurableTestMessage, ProtobufNetSerializer>()))
                            .AddProducer<RetryDurableGuaranteeOrderedConsumptionSqlServerProducer>(
                                producer => producer
                                    .DefaultTopic("test-retry-durable-guarantee-ordered-consumption-sql-server")
                                    .WithCompression(Confluent.Kafka.CompressionType.Gzip)
                                    .AddMiddlewares(
                                        middlewares => middlewares
                                            .AddSingleTypeSerializer<RetryDurableTestMessage, ProtobufNetSerializer>()))

                            .AddConsumer(
                                consumer => consumer
                                    .Topic(RetrySimpleTopicName)
                                    .WithGroupId("test-consumer-retry-simple")
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
                                                        .AddHandler<RetrySimpleTestMessageHandler>())))
                            .AddConsumer(
                                consumer => consumer
                                    .Topic(RetryForeverTopicName)
                                    .WithGroupId("test-consumer-retry-forever")
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
                                                        .AddHandler<RetryForeverTestMessageHandler>())))
                            .AddConsumer(
                                consumer => consumer
                                    .Topic("test-retry-durable-guarantee-ordered-consumption-mongo-db")
                                    .WithGroupId("test-consumer-retry-durable-guarantee-ordered-consumption-mongo-db")
                                    .WithBufferSize(100)
                                    .WithWorkersCount(10)
                                    .WithAutoOffsetReset(AutoOffsetReset.Latest)
                                    .AddMiddlewares(
                                        middlewares => middlewares
                                            .AddSingleTypeSerializer<ProtobufNetSerializer>(typeof(RetryDurableTestMessage))
                                            .RetryDurable(
                                                (configure) => configure
                                                    .Handle<RetryDurableTestException>()
                                                    .WithMessageType(typeof(RetryDurableTestMessage))
                                                    .WithEmbeddedRetryCluster(
                                                        cluster,
                                                        configure => configure
                                                            .Enabled(true)
                                                            .WithRetryTopicName("test-retry-durable-guarantee-ordered-consumption-mongo-db-retry")
                                                            .WithRetryConsumerBufferSize(100)
                                                            .WithRetryConsumerWorkersCount(10)
                                                            .WithRetryConusmerStrategy(RetryConsumerStrategy.GuaranteeOrderedConsumption)
                                                            .WithRetryTypedHandlers(
                                                                handlers => handlers
                                                                    .WithHandlerLifetime(InstanceLifetime.Transient)
                                                                    .AddHandler<RetryDurableTestMessageHandler>()))
                                                    .WithQueuePollingJobConfiguration(
                                                        configure => configure
                                                            .Enabled(true)
                                                            .WithId("custom_search_key_durable_mongo_db")
                                                            .WithCronExpression("0/15 * * ? * * *")
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
                                                        .AddHandler<RetryDurableTestMessageHandler>())))
                            .AddConsumer(
                                consumer => consumer
                                    .Topic("test-retry-durable-guarantee-ordered-consumption-sql-server")
                                    .WithGroupId("test-consumer-retry-durable-guarantee-ordered-consumption-sql-server")
                                    .WithBufferSize(100)
                                    .WithWorkersCount(10)
                                    .WithAutoOffsetReset(AutoOffsetReset.Latest)
                                    .AddMiddlewares(
                                        middlewares => middlewares
                                            .AddSingleTypeSerializer<ProtobufNetSerializer>(typeof(RetryDurableTestMessage))
                                            .RetryDurable(
                                                (configure) => configure
                                                    .Handle<RetryDurableTestException>()
                                                    .WithMessageType(typeof(RetryDurableTestMessage))
                                                    .WithEmbeddedRetryCluster(
                                                        cluster,
                                                        configure => configure
                                                            .Enabled(true)
                                                            .WithRetryTopicName("test-retry-durable-guarantee-ordered-consumption-sql-server-retry")
                                                            .WithRetryConsumerBufferSize(100)
                                                            .WithRetryConsumerWorkersCount(10)
                                                            .WithRetryConusmerStrategy(RetryConsumerStrategy.GuaranteeOrderedConsumption)
                                                            .WithRetryTypedHandlers(
                                                                handlers => handlers
                                                                    .WithHandlerLifetime(InstanceLifetime.Transient)
                                                                    .AddHandler<RetryDurableTestMessageHandler>()))
                                                    .WithQueuePollingJobConfiguration(
                                                        configure => configure
                                                            .Enabled(true)
                                                            .WithId("custom_search_key_durable_sql_server")
                                                            .WithCronExpression("0/15 * * ? * * *")
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
                                                        .AddHandler<RetryDurableTestMessageHandler>())))
                                    ));

            services.AddSingleton<RetrySimpleProducer>();
            services.AddSingleton<RetryForeverProducer>();
            services.AddSingleton<RetryDurableGuaranteeOrderedConsumptionMongoDbProducer>();
            services.AddSingleton<RetryDurableGuaranteeOrderedConsumptionSqlServerProducer>();
            services.AddSingleton(
                new SqlServerStorage(
                    sqlServerConnectionString,
                    sqlServerDatabaseName));
            services.AddSingleton(
                new MongoStorage(
                    mongoDbConnectionString,
                    mongoDbDatabaseName,
                    mongoDbRetryQueueCollectionName,
                    mongoDbRetryQueueItemCollectionName));
        }
    }
}