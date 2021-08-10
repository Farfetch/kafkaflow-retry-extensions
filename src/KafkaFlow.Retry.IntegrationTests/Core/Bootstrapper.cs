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
    using KafkaFlow.Retry.MongoDb;
    using KafkaFlow.Serializer;
    using KafkaFlow.TypedHandler;
    using AutoOffsetReset = KafkaFlow.AutoOffsetReset;

    internal static class Bootstrapper
    {
        private const string RetryDurableTopicName = "test-retry-durable";
        private const string RetryForeverTopicName = "test-retry-forever";
        private const string RetrySimpleTopicName = "test-retry-simple";
        private static readonly Lazy<IServiceProvider> LazyProvider = new Lazy<IServiceProvider>(SetupProvider);

        private static IKafkaBus kafkaBus;

        public static IServiceProvider GetServiceProvider() => LazyProvider.Value;

        public static void KafkaBusStop() => kafkaBus.StopAsync().GetAwaiter().GetResult();

        private static IServiceProvider SetupProvider()
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

            return host.Services;
        }

        private static void SetupServices(HostBuilderContext context, IServiceCollection services)
        {
            var kafkaBrokers = context.Configuration.GetValue<string>("Kafka:Brokers");
            var mongoDbconnectionString = context.Configuration.GetValue<string>("MongoDbRepository:ConnectionString");
            var mongoDbdatabaseName = context.Configuration.GetValue<string>("MongoDbRepository:DatabaseName");
            var mongoDbretryQueueCollectionName = context.Configuration.GetValue<string>("MongoDbRepository:RetryQueueCollectionName");
            var mongoDbretryQueueItemCollectionName = context.Configuration.GetValue<string>("MongoDbRepository:RetryQueueItemCollectionName");

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
                            .AddProducer<RetryDurableProducer>(
                                producer => producer
                                    .DefaultTopic(RetryDurableTopicName)
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
                                    .Topic(RetryDurableTopicName)
                                    .WithGroupId("test-consumer-retry-durable")
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
                                                            .WithRetryTopicName("test-consumer-retry-durable-retry")
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
                                                            .WithId("custom_search_key_durable")
                                                            .WithCronExpression("0/30 * * ? * * *")
                                                            .WithExpirationIntervalFactor(1)
                                                            .WithFetchSize(20))
                                                    .WithMongoDbDataProvider(
                                                        mongoDbconnectionString,
                                                        mongoDbdatabaseName,
                                                        mongoDbretryQueueCollectionName,
                                                        mongoDbretryQueueItemCollectionName)
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
            services.AddSingleton<RetryDurableProducer>();

            MongoStorage
                .Setup(
                    mongoDbconnectionString,
                    mongoDbdatabaseName,
                    mongoDbretryQueueCollectionName,
                    mongoDbretryQueueItemCollectionName);
        }
    }
}