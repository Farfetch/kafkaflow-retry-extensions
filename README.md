# KafkaFlow Retry Extensions
KafkaFlow Retry is a .NET framework to retry messages on consumers, simple to use.

KafkaFlow Retry is an extention of [Kafka Flow](https://github.com/Farfetch/kafka-flow).

## KafkaFlow Retry

## Features
 - Simple Retry
 - Forever Retry
 - Durable Retry
 - Fluent configuration
 - Admin Web API to manage messages and queue messages
 - Persistence in SQL Server and MongoDb

## Packages

|Name                             |nuget.org|
|---------------------------------|----|
|KafkaFlow.Retry|[![Nuget Package](https://img.shields.io/nuget/v/KafkaFlow.Retry.svg?logo=nuget)](https://www.nuget.org/packages/KafkaFlow.Retry/) ![Nuget downloads](https://img.shields.io/nuget/dt/KafkaFlow.Retry.svg)
|KafkaFlow.Retry.API|[![Nuget Package](https://img.shields.io/nuget/v/KafkaFlow.Retry.API.svg?logo=nuget)](https://www.nuget.org/packages/KafkaFlow.Retry.API/) ![Nuget downloads](https://img.shields.io/nuget/dt/KafkaFlow.Retry.API.svg)
|KafkaFlow.Retry.MongoDb|[![Nuget Package](https://img.shields.io/nuget/v/KafkaFlow.Retry.MongoDb.svg?logo=nuget)](https://www.nuget.org/packages/KafkaFlow.Retry.MongoDb/) ![Nuget downloads](https://img.shields.io/nuget/dt/KafkaFlow.Retry.MongoDb.svg)
|KafkaFlow.Retry.SqlServer|[![Nuget Package](https://img.shields.io/nuget/v/KafkaFlow.Retry.SqlServer.svg?logo=nuget)](https://www.nuget.org/packages/KafkaFlow.Retry.SqlServer/) ![Nuget downloads](https://img.shields.io/nuget/dt/KafkaFlow.Retry.SqlServer.svg)

## Usage Examples

**.NET Core 2.1 and later using Hosted Service**

### Simple Retry
```csharp
public static void Main(string[] args)
{
    Host
        .CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
        {
            services.AddKafkaFlowHostedService(kafka => kafka
                .UseConsoleLog()
                .AddCluster(cluster => cluster
                    .WithBrokers(new[] { "localhost:9092" })
                    .AddConsumer(consumer => consumer
                        .Topic("sample-topic")
                        .WithGroupId("sample-group")
                        .WithBufferSize(100)
                        .WithWorkersCount(10)
                        .AddMiddlewares(middlewares => middlewares
                            .AddSerializer<NewtonsoftJsonMessageSerializer>()
                            .RetryForever(
                                (configure) => configure
                                    .Handle<CostumException>()
                                    .Handle<TimeoutException>()
                                    .WithTimeBetweenTriesPlan(
                                        TimeSpan.FromMilliseconds(500),
                                        TimeSpan.FromMilliseconds(1000))
                            .AddTypedHandlers(handlers => handlers
                                .AddHandler<SampleMessageHandler>())
                        )
                    )
                    .AddProducer("producer-name", producer => producer
                        .DefaultTopic("sample-topic")
                        .AddMiddlewares(middlewares => middlewares
                            .AddSerializer<NewtonsoftJsonMessageSerializer>()
                        )
                    )
                )
            );
        })
        .Build()
        .Run();
}
```

### Durable Retry
```csharp
public static void Main(string[] args)
{
    Host
        .CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
        {
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
                                   .WithGroupId("application-group-id")
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
                                    )
                           )
                   )
           )
           .Build()
           .Run();
     };
```

See the [setup page](https://github.com/Farfetch/kafka-flow-retry-extensions/wiki/Setup) and [samples](/samples) for more details

## Documentation

[Wiki Page](https://github.com/Farfetch/kafka-flow-retry-extensions/wiki)

## Contributing

Read the [Contributing guidelines](CONTRIBUTING.md)

## Maintainers

-   [Carlos Goias](https://github.com/carlosgoias)
-   [Fernando Marins](https://github.com/fernando-a-marins)
-   [Luís Garcês](https://github.com/luispfgarces)
-   [Martinho Novais](https://github.com/martinhonovais)
-   [Rodrigo Belo](https://github.com/rodrigobelo)


## License

[MIT](LICENSE.md)
