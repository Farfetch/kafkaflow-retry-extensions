using System;
using System.Threading;
using System.Threading.Tasks;
using global::SchemaRegistry;
using KafkaFlow.Producers;
using KafkaFlow.Retry.Common.Sample.Helpers;
using KafkaFlow.Retry.SchemaRegistry.Sample.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace KafkaFlow.Retry.SchemaRegistry.Sample;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var services = new ServiceCollection();
        var brokers = "localhost:9092";
        var mongoDbConnectionString = "mongodb://localhost:27017";
        var mongoDbDatabaseName = "kafka_flow_retry_durable_sample";
        var mongoDbRetryQueueCollectionName = "RetryQueues";
        var mongoDbRetryQueueItemCollectionName = "RetryQueueItems";

        var topics = new[]
        {
            "sample-kafka-flow-retry-durable-mongodb-avro-topic",
            "sample-kafka-flow-retry-durable-mongodb-avro-topic-retry",
        };

        KafkaHelper.CreateKafkaTopics(brokers, topics).GetAwaiter().GetResult();

        services.AddKafka(
            kafka => kafka
                .UseConsoleLog()
                .AddCluster(
                    cluster => cluster
                        .WithBrokers(new[] { brokers })
                        .WithSchemaRegistry(config => config.Url = "localhost:8085")
                        .SetupRetryDurableMongoAvroDb(
                            mongoDbConnectionString,
                            mongoDbDatabaseName,
                            mongoDbRetryQueueCollectionName,
                            mongoDbRetryQueueItemCollectionName)
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
            Console.WriteLine("Number of messages to produce or exit:");
            var input = Console.ReadLine().ToLower();

            switch (input)
            {
                case var _ when int.TryParse(input, out var count):
                    for (var i = 0; i < count; i++)
                    {
                        await Task.WhenAll(
                            producers["kafka-flow-retry-durable-mongodb-avro-producer"].ProduceAsync(
                                Guid.NewGuid().ToString(),
                                new AvroLogMessage { Severity = LogLevel.Info })
                        );
                    }

                    break;

                case "exit":
                    await bus.StopAsync();
                    return;
            }
        }
    }
}