namespace KafkaFlow.Retry.Sample
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using global::Microsoft.Extensions.DependencyInjection;
    using KafkaFlow;
    using KafkaFlow.Admin;
    using KafkaFlow.Admin.Messages;
    using KafkaFlow.Consumers;
    using KafkaFlow.Producers;
    using KafkaFlow.Retry;
    using KafkaFlow.Retry.MongoDb;
    using KafkaFlow.Serializer;
    using KafkaFlow.TypedHandler;

    internal static class Program
    {
        private static async Task Main()
        {
            var services = new ServiceCollection();

            const string producerName = "PrintConsole";

            const string consumerName = "test";
            const int TimeoutErrorCode = -2;
            const int ServerIsInaccessible = -2146232060;

            const string sqlServerConnectionString = "Server=localhost;Database=SVC_KAFKA_FLOW_RETRY_DURABLE;Trusted_Connection=True; Pooling=true; Min Pool Size=1; Max Pool Size=100; MultipleActiveResultSets=true; Application Name=Finance Transactions Journal Service";
            const string sqlServerName = "SVC_KAFKA_FLOW_RETRY_DURABLE";

            const string mongoDbconnectionString = "mongodb://localhost:27017/SVC_KAFKA_FLOW_RETRY_DURABLE";
            const string mongoDbdatabaseName = "SVC_KAFKA_FLOW_RETRY_DURABLE";
            const string mongoDbretryQueueCollectionName = "RetryQueues";
            const string mongoDbretryQueueItemCollectionName = "RetryQueueItems";

            services.AddKafka(
                    kafka => kafka
                        .UseConsoleLog()
                        .AddCluster(
                            cluster => cluster
                                .WithBrokers(new[] { "localhost:9092" })
                                .EnableAdminMessages("kafka-flow.admin", Guid.NewGuid().ToString())
                                .AddProducer(
                                    producerName,
                                    producer => producer
                                        .DefaultTopic("test-topic")
                                        .WithCompression(Confluent.Kafka.CompressionType.Gzip)
                                        .AddMiddlewares(
                                            middlewares => middlewares
                                                .AddSerializer<ProtobufNetSerializer>()
                                        )
                                        .WithAcks(Acks.All)
                                )
                                .AddConsumer(
                                    consumer => consumer
                                        .Topic("test-topic")
                                        .WithGroupId("print-console-handler")
                                        .WithName(consumerName)
                                        .WithBufferSize(10)
                                        .WithWorkersCount(20)
                                        .WithAutoOffsetReset(AutoOffsetReset.Latest)
                                        .AddMiddlewares(
                                            middlewares => middlewares
                                                .AddSerializer<ProtobufNetSerializer>()
                                                .RetryDurable(
                                                    configure => configure
                                                        .Handle<NonBlockingException>()
                                                        .WithMessageType(typeof(TestMessage))
                                                        .WithEmbeddedRetryCluster(
                                                            cluster,
                                                            configure => configure
                                                                .WithRetryTopicName("test-topic-retry")
                                                                .WithRetryConsumerBufferSize(4)
                                                                .WithRetryConsumerWorkersCount(2)
                                                                .WithRetryConusmerStrategy(RetryConsumerStrategy.GuaranteeOrderedConsumption)
                                                                .WithRetryTypedHandlers(
                                                                    handlers => handlers
                                                                        .WithHandlerLifetime(InstanceLifetime.Transient)
                                                                        .AddHandler<Handler>()
                                                                )
                                                                .Enabled(true)
                                                        )
                                                        .WithQueuePollingJobConfiguration(
                                                            configure => configure
                                                                .WithId("custom_search_key")
                                                                .WithCronExpression("0 0/1 * 1/1 * ? *")
                                                                .WithExpirationIntervalFactor(1)
                                                                .WithFetchSize(10)
                                                                .Enabled(true)
                                                        )
                                                        //.WithSqlServerDataProvider(sqlServerConnectionString, sqlServerName)
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
                                                                .ShouldPauseConsumer(false)
                                                        )
                                                )
                                                .RetrySimple(
                                                    (configure) => configure
                                                        .Handle<CustomException>()
                                                        .TryTimes(2)
                                                        .WithTimeBetweenTriesPlan((retryCount) =>
                                                        {
                                                            var plan = new[]
                                                            {
                                                            TimeSpan.FromMilliseconds(1500),
                                                            TimeSpan.FromMilliseconds(2000)
                                                            };

                                                            return plan[retryCount];
                                                        })
                                                        .ShouldPauseConsumer(false)
                                                )
                                                .RetryForever(
                                                    (configure) => configure
                                                        .Handle<CustomException>()
                                                        //.Handle<SqlException>(ex => ex.ErrorCode == TimeoutErrorCode)
                                                        //.Handle<SqlException>(ex => ex.ErrorCode == ServerIsInaccessible)
                                                        .WithTimeBetweenTriesPlan(
                                                            TimeSpan.FromMilliseconds(500),
                                                            TimeSpan.FromMilliseconds(1000))
                                                )
                                                .AddTypedHandlers(
                                                    handlers => handlers
                                                        .WithHandlerLifetime(InstanceLifetime.Transient)
                                                        .AddHandler<Handler>())
                                        )
                                )
                        )
                );

            var provider = services.BuildServiceProvider();

            var bus = provider.CreateKafkaBus();

            await bus.StartAsync();

            var consumers = provider.GetRequiredService<IConsumerAccessor>();
            var producers = provider.GetRequiredService<IProducerAccessor>();

            var adminProducer = provider.GetService<IAdminProducer>();

            while (true)
            {
                Console.Write("Number of messages to produce, Pause, Resume, or Exit:");
                var input = Console.ReadLine().ToLower();

                switch (input)
                {
                    case var _ when int.TryParse(input, out var count):
                        await producers[producerName]
                            .BatchProduceAsync(
                                Enumerable
                                    .Range(0, count)
                                    .Select(
                                        x => new BatchProduceItem(
                                            "test-topic",
                                            "same-key",
                                            new TestMessage { Text = $"Message: {Guid.NewGuid()}" },
                                            null))
                                    .ToList());

                        break;

                    case "pause":
                        foreach (var consumer in consumers.All)
                        {
                            consumer.Pause(consumer.Assignment);
                        }

                        Console.WriteLine("Consumer paused");

                        break;

                    case "resume":
                        foreach (var consumer in consumers.All)
                        {
                            consumer.Resume(consumer.Assignment);
                        }

                        Console.WriteLine("Consumer resumed");

                        break;

                    case "reset":
                        await adminProducer.ProduceAsync(new ResetConsumerOffset { ConsumerName = consumerName });

                        break;

                    case "rewind":
                        Console.Write("Input a time: ");
                        var timeInput = Console.ReadLine();

                        if (DateTime.TryParse(timeInput, out var time))
                        {
                            await adminProducer.ProduceAsync(
                                new RewindConsumerOffsetToDateTime
                                {
                                    ConsumerName = consumerName,
                                    DateTime = time
                                });
                        }

                        break;

                    case "workers":
                        Console.Write("Input a new worker count: ");
                        var workersInput = Console.ReadLine();

                        if (int.TryParse(workersInput, out var workers))
                        {
                            await adminProducer.ProduceAsync(
                                new ChangeConsumerWorkersCount
                                {
                                    ConsumerName = consumerName,
                                    WorkersCount = workers
                                });
                        }

                        break;

                    case "exit":
                        await bus.StopAsync();
                        return;
                }
            }
        }
    }
}