namespace KafkaFlow.Retry.Sample
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using global::Microsoft.Extensions.DependencyInjection;
    using KafkaFlow;
    using KafkaFlow.Admin;
    using KafkaFlow.Configuration;
    using KafkaFlow.Consumers;
    using KafkaFlow.Producers;
    using KafkaFlow.Retry;
    using KafkaFlow.Retry.MongoDb;
    using KafkaFlow.Retry.Sample.Exceptions;
    using KafkaFlow.Retry.Sample.Handlers;
    using KafkaFlow.Retry.Sample.Messages;
    using KafkaFlow.Retry.SqlServer;
    using KafkaFlow.Serializer;
    using KafkaFlow.TypedHandler;

    internal static class Program
    {
        private static async Task Main()
        {
            var services = new ServiceCollection();

            services.AddKafka(
                kafka => kafka
                    .UseConsoleLog()
                    .AddCluster(
                        cluster => cluster
                            .WithBrokers(new[] { "localhost:9092" })
                            .EnableAdminMessages("kafka-flow.admin", Guid.NewGuid().ToString())
                            .SetupRetrySimple()
                            .SetupRetryForever()
                            .SetupRetryDurableMongoDb()
                            .SetupRetryDurableSqlServer()
                    )
                );

            var provider = services.BuildServiceProvider();

            var bus = provider.CreateKafkaBus();

            await bus.StartAsync();

            // Wait partition assignment
            Thread.Sleep(10000);

            var consumers = provider.GetRequiredService<IConsumerAccessor>();
            var producers = provider.GetRequiredService<IProducerAccessor>();

            var adminProducer = provider.GetService<IAdminProducer>();

            while (true)
            {
                Console.Write("retry-simple, retry-forever, retry-durable-mongodb or retry-durable-sqlserver: ");
                var input = Console.ReadLine().ToLower();

                switch (input)
                {
                    case "retry-durable-mongodb":
                        {
                            Console.Write("Number of the distinct messages to produce: ");
                            int.TryParse(Console.ReadLine().ToLower(), out var numOfMessages);
                            Console.Write("Number of messages with same partition key: ");
                            int.TryParse(Console.ReadLine().ToLower(), out var numOfMessagesWithSamePartitionkey);
                            var messages = Enumerable
                                .Range(0, numOfMessages)
                                .SelectMany(
                                    x =>
                                    {
                                        var partitionKey = Guid.NewGuid().ToString();
                                        return Enumerable
                                            .Range(0, numOfMessagesWithSamePartitionkey)
                                            .Select(y => new BatchProduceItem(
                                                "test-kafka-flow-retry-durable-mongodb-topic",
                                                partitionKey,
                                                new RetryDurableTestMessage { Text = $"Message({y}): {Guid.NewGuid()}" },
                                                null))
                                            .ToList();
                                    }
                                )
                                .ToList();

                            await producers["kafka-flow-retry-durable-mongodb-producer"]
                                .BatchProduceAsync(messages)
                                .ConfigureAwait(false);
                            Console.WriteLine("Published");
                        }
                        break;

                    case "retry-durable-sqlserver":
                        {
                            Console.Write("Number of the distinct messages to produce: ");
                            int.TryParse(Console.ReadLine().ToLower(), out var numOfMessages);
                            Console.Write("Number of messages with same partition key: ");
                            int.TryParse(Console.ReadLine().ToLower(), out var numOfMessagesWithSamePartitionkey);

                            var messages = Enumerable
                                .Range(0, numOfMessages)
                                .SelectMany(
                                    x =>
                                    {
                                        var partitionKey = Guid.NewGuid().ToString();
                                        return Enumerable
                                            .Range(0, numOfMessagesWithSamePartitionkey)
                                            .Select(y => new BatchProduceItem(
                                                "test-kafka-flow-retry-durable-sqlserver-topic",
                                                partitionKey,
                                                new RetryDurableTestMessage { Text = $"Message({y}): {Guid.NewGuid()}" },
                                                null))
                                            .ToList();
                                    }
                                )
                                .ToList();

                            await producers["kafka-flow-retry-durable-sqlserver-producer"]
                                .BatchProduceAsync(messages)
                                .ConfigureAwait(false);
                            Console.WriteLine("Published");
                        }
                        break;

                    case "retry-forever":
                        {
                            Console.Write("Number of messages to produce: ");
                            int.TryParse(Console.ReadLine().ToLower(), out var num_of_messages);
                            await producers["kafka-flow-retry-forever-producer"]
                                .BatchProduceAsync(
                                    Enumerable
                                        .Range(0, num_of_messages)
                                        .Select(
                                            x => new BatchProduceItem(
                                                "test-kafka-flow-retry-forever-topic",
                                                "partition-key",
                                                new RetryForeverTestMessage { Text = $"Message({x}): {Guid.NewGuid()}" },
                                                null))
                                        .ToList())
                                .ConfigureAwait(false);
                            Console.WriteLine("Published");
                        }
                        break;

                    case "retry-simple":
                        {
                            Console.Write("Number of messages to produce:");
                            int.TryParse(Console.ReadLine().ToLower(), out var num_of_messages);
                            await producers["kafka-flow-retry-simple-producer"]
                                .BatchProduceAsync(
                                    Enumerable
                                        .Range(0, num_of_messages)
                                        .Select(
                                            x => new BatchProduceItem(
                                                "test-kafka-flow-retry-simple-topic",
                                                "partition-key",
                                                new RetrySimpleTestMessage { Text = $"Message({x}): {Guid.NewGuid()}" },
                                                null))
                                        .ToList())
                                .ConfigureAwait(false);
                            Console.WriteLine("Published");
                        }
                        break;

                    case "exit":
                        await bus.StopAsync();
                        return;
                }
            }
        }

        private static IClusterConfigurationBuilder SetupRetryDurableMongoDb(this IClusterConfigurationBuilder cluster)
        {
            cluster
                .AddProducer(
                    "kafka-flow-retry-durable-mongodb-producer",
                    producer => producer
                        .DefaultTopic("test-kafka-flow-retry-durable-mongodb-topic")
                        .WithCompression(Confluent.Kafka.CompressionType.Gzip)
                        .AddMiddlewares(
                            middlewares => middlewares
                                .AddSerializer<ProtobufNetSerializer>()
                        )
                        .WithAcks(Acks.All)
                )
                .AddConsumer(
                    consumer => consumer
                        .Topic("test-kafka-flow-retry-durable-mongodb-topic")
                        .WithGroupId("test-kafka-flow-retry-durable-mongodb")
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
                                            "mongodb://localhost:27017/svc_kafka_flow_retry_durable",
                                            "kafka_flow_retry_durable",
                                            "RetryQueues",
                                            "RetryQueueItems")
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
                                                .WithRetryTopicName("test-kafka-flow-retry-durable-mongodb-topic-retry")
                                                .WithRetryConsumerBufferSize(4)
                                                .WithRetryConsumerWorkersCount(2)
                                                .WithRetryConusmerStrategy(RetryConsumerStrategy.GuaranteeOrderedConsumption)
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

        private static IClusterConfigurationBuilder SetupRetryDurableSqlServer(this IClusterConfigurationBuilder cluster)
        {
            cluster
                .AddProducer(
                    "kafka-flow-retry-durable-sqlserver-producer",
                    producer => producer
                        .DefaultTopic("test-kafka-flow-retry-durable-sqlserver-topic")
                        .WithCompression(Confluent.Kafka.CompressionType.Gzip)
                        .AddMiddlewares(
                            middlewares => middlewares
                                .AddSerializer<ProtobufNetSerializer>()
                        )
                        .WithAcks(Acks.All)
                )
                .AddConsumer(
                    consumer => consumer
                        .Topic("test-kafka-flow-retry-durable-sqlserver-topic")
                        .WithGroupId("test-kafka-flow-retry-durable-sqlserver")
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
                                            "Server=localhost;Database=kafka_flow_retry_durable;Trusted_Connection=True; Pooling=true; Min Pool Size=1; Max Pool Size=100; MultipleActiveResultSets=true; Application Name=KafkaFlow Retry Sample",
                                            "kafka_flow_retry_durable")
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
                                                .WithRetryTopicName("test-kafka-flow-retry-durable-sqlserver-topic-retry")
                                                .WithRetryConsumerBufferSize(4)
                                                .WithRetryConsumerWorkersCount(2)
                                                .WithRetryConusmerStrategy(RetryConsumerStrategy.LatestConsumption)
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

        private static IClusterConfigurationBuilder SetupRetryForever(this IClusterConfigurationBuilder cluster)
        {
            cluster
                .AddProducer(
                    "kafka-flow-retry-forever-producer",
                    producer => producer
                        .DefaultTopic("test-kafka-flow-retry-forever-topic")
                        .WithCompression(Confluent.Kafka.CompressionType.Gzip)
                        .AddMiddlewares(
                            middlewares => middlewares
                                .AddSerializer<ProtobufNetSerializer>()
                        )
                        .WithAcks(Acks.All)
                )
                .AddConsumer(
                    consumer => consumer
                        .Topic("test-kafka-flow-retry-forever-topic")
                        .WithGroupId("test-kafka-flow-retry-forever")
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

        private static IClusterConfigurationBuilder SetupRetrySimple(this IClusterConfigurationBuilder cluster)
        {
            cluster
                .AddProducer(
                    "kafka-flow-retry-simple-producer",
                    producer => producer
                        .DefaultTopic("test-kafka-flow-retry-simple-topic")
                        .WithCompression(Confluent.Kafka.CompressionType.Gzip)
                        .AddMiddlewares(
                            middlewares => middlewares
                                .AddSerializer<ProtobufNetSerializer>()
                        )
                        .WithAcks(Acks.All)
                )
                .AddConsumer(
                    consumer => consumer
                        .Topic("test-kafka-flow-retry-simple-topic")
                        .WithGroupId("test-kafka-flow-retry-simple")
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