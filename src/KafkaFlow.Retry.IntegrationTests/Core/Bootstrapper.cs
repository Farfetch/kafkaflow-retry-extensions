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
    using KafkaFlow.Serializer;
    using KafkaFlow.TypedHandler;
    using AutoOffsetReset = KafkaFlow.AutoOffsetReset;

    internal static class Bootstrapper
    {
        private const string RetryForeverTopicName = "test-retry-forever";
        private const string RetrySimpleTopicName = "test-retry-simple";
        private static readonly Lazy<IServiceProvider> LazyProvider = new Lazy<IServiceProvider>(SetupProvider);

        public static IServiceProvider GetServiceProvider() => LazyProvider.Value;

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
            var bus = host.Services.CreateKafkaBus();
            bus.StartAsync().GetAwaiter().GetResult();

            // Wait partition assignment
            Thread.Sleep(10000);

            return host.Services;
        }

        private static void SetupServices(HostBuilderContext context, IServiceCollection services)
        {
            var kafkaBrokers = context.Configuration.GetValue<string>("Kafka:Brokers");

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
                                                    .Handle<RetrySimpleException>()
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
                                                    .Handle<RetryForeverException>()
                                                    .WithTimeBetweenTriesPlan(TimeSpan.FromMilliseconds(100)))
                                            .AddTypedHandlers(
                                                handlers =>
                                                    handlers
                                                        .WithHandlerLifetime(InstanceLifetime.Singleton)
                                                        .AddHandler<RetryForeverTestMessageHandler>())))
                                    ));

            services.AddSingleton<RetrySimpleProducer>();
        }
    }
}