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


## Basic Usage

**.NET Core 2.1 and later using Hosted Service**

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
See the [setup page](https://github.com/Farfetch/kafka-flow-retry-extensions/wiki/Setup) and [samples](/samples) for more details

## Documentation

[Wiki Page](https://github.com/Farfetch/kafka-flow-retry-extensions/wiki)

## Contributing

Read the [Contributing guidelines](CONTRIBUTING.md)

## Maintainers

-   [Rodrigo Belo](https://github.com/rodrigobelo)
-   [Martinho Novais](https://github.com/martinhonovais)
-   [Luís Garcês](https://github.com/luispfgarces)

## License

[MIT](LICENSE.md)
