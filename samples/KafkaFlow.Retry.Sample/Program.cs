namespace KafkaFlow.Retry.Sample
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using global::Microsoft.Extensions.DependencyInjection;
    using KafkaFlow;
    using KafkaFlow.Producers;
    using KafkaFlow.Retry.Sample.Helpers;
    using KafkaFlow.Retry.Sample.Messages;

    internal static class Program
    {
        private static async Task Main()
        {
            var services = new ServiceCollection();
            var brokers = "localhost:9092";
            var mongoDbConnectionString = "mongodb://localhost:27017";
            var mongoDbDatabaseName = "kafka_flow_retry_durable_sample";
            var mongoDbRetryQueueCollectionName = "RetryQueues";
            var mongoDbRetryQueueItemCollectionName = "RetryQueueItems";
            var sqlServerConnectionString = "Server=localhost;Trusted_Connection=True; Pooling=true; Min Pool Size=1; Max Pool Size=100; MultipleActiveResultSets=true; Application Name=KafkaFlow Retry Sample";
            var sqlServerDatabaseName = "kafka_flow_retry_durable_sample";
            var topics = new[]
            {
                "sample-kafka-flow-retry-simple-topic",
                "sample-kafka-flow-retry-forever-topic",
                "sample-kafka-flow-retry-durable-sqlserver-topic",
                "sample-kafka-flow-retry-durable-sqlserver-topic-retry",
                "sample-kafka-flow-retry-durable-mongodb-topic",
                "sample-kafka-flow-retry-durable-mongodb-topic-retry",
            };

            SqlServerHelper.RecreateSqlSchema(sqlServerDatabaseName, sqlServerConnectionString).GetAwaiter().GetResult();
            KafkaHelper.CreateKafkaTopics(brokers, topics).GetAwaiter().GetResult();

            services.AddKafka(
                kafka => kafka
                    .UseConsoleLog()
                    .AddCluster(
                        cluster => cluster
                            .WithBrokers(new[] { brokers })
                            .EnableAdminMessages("kafka-flow.admin", Guid.NewGuid().ToString())
                            .SetupRetrySimple()
                            .SetupRetryForever()
                            .SetupRetryDurableMongoDb(
                                mongoDbConnectionString,
                                mongoDbDatabaseName,
                                mongoDbRetryQueueCollectionName,
                                mongoDbRetryQueueItemCollectionName)
                            .SetupRetryDurableSqlServer(
                                sqlServerConnectionString,
                                sqlServerDatabaseName)
                    )
                );

            var provider = services.BuildServiceProvider();

            var bus = provider.CreateKafkaBus();

            await bus.StartAsync();

            // Wait partition assignment
            Thread.Sleep(10000);

            var producers = provider.GetRequiredService<IProducerAccessor>();

            while (true)
            {
                Console.Write("retry-simple, retry-forever, retry-durable-mongodb, retry-durable-sqlserver or exit: ");
                var input = Console.ReadLine().ToLower(CultureInfo.InvariantCulture);

                switch (input)
                {
                    case "retry-durable-mongodb":
                        {
                            Console.Write("Number of the distinct messages to produce: ");
                            int.TryParse(Console.ReadLine(), out var numOfMessages);
                            Console.Write("Number of messages with same partition key: ");
                            int.TryParse(Console.ReadLine(), out var numOfMessagesWithSamePartitionkey);
                            var messages = Enumerable
                                .Range(0, numOfMessages)
                                .SelectMany(
                                    x =>
                                    {
                                        var partitionKey = Guid.NewGuid().ToString();
                                        return Enumerable
                                            .Range(0, numOfMessagesWithSamePartitionkey)
                                            .Select(y => new BatchProduceItem(
                                                "sample-kafka-flow-retry-durable-mongodb-topic",
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
                            int.TryParse(Console.ReadLine(), out var numOfMessages);
                            Console.Write("Number of messages with same partition key: ");
                            int.TryParse(Console.ReadLine(), out var numOfMessagesWithSamePartitionkey);

                            var messages = Enumerable
                                .Range(0, numOfMessages)
                                .SelectMany(
                                    x =>
                                    {
                                        var partitionKey = Guid.NewGuid().ToString();
                                        return Enumerable
                                            .Range(0, numOfMessagesWithSamePartitionkey)
                                            .Select(y => new BatchProduceItem(
                                                "sample-kafka-flow-retry-durable-sqlserver-topic",
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
                            int.TryParse(Console.ReadLine(), out var num_of_messages);
                            await producers["kafka-flow-retry-forever-producer"]
                                .BatchProduceAsync(
                                    Enumerable
                                        .Range(0, num_of_messages)
                                        .Select(
                                            x => new BatchProduceItem(
                                                "sample-kafka-flow-retry-forever-topic",
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
                            int.TryParse(Console.ReadLine(), out var num_of_messages);
                            await producers["kafka-flow-retry-simple-producer"]
                                .BatchProduceAsync(
                                    Enumerable
                                        .Range(0, num_of_messages)
                                        .Select(
                                            x => new BatchProduceItem(
                                                "sample-kafka-flow-retry-simple-topic",
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
                        break;

                    default:
                        Console.Write("USE: retry-simple, retry-forever, retry-durable-mongodb, retry-durable-sqlserver or exit: ");
                        break;
                }
            }
        }
    }
}