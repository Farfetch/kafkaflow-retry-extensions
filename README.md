# KafkaFlow Retry Extensions
KafkaFlow Retry is a .NET framework to retry messages on consumers, simple to use.

KafkaFlow Retry is an extention of [Kafka Flow](https://github.com/Farfetch/kafka-flow).


# Resilience policies

|Policy| Description | Aka| Required Packages|
| ------------- | ------------- |:-------------: |------------- |
|**Simple Retry** <br/>(policy family)<br/><sub>([quickstart](#retry)&nbsp;;&nbsp;[deep](https://github.com/App-vNext/Polly/wiki/Retry))</sub>|Many faults are transient and may self-correct after a short delay.| "Maybe it's just a blip" |   KafkaFlow.Retry |
|**Forever Retry**<br/>(policy family)<br/><sub>([quickstart](#circuit-breaker)&nbsp;;&nbsp;[deep](https://github.com/App-vNext/Polly/wiki/Circuit-Breaker))</sub>|Many faults are semi-transient and may self-correct after multiple retries. | "Never give up" | KafkaFlow.Retry | 
|**Durable Retry**<br/><sub>([quickstart](#timeout)&nbsp;;&nbsp;[deep](https://github.com/App-vNext/Polly/wiki/Timeout))</sub>|Beyond a certain amount of retries and wait, you want to keep processing next-in-line messages but you can't loss the current offset message. As persistance databases, MongoDb or SqlServer are available. And you can manage in-retry messages through HTTP API.| "I can't stop processing messages but I can't loss messages"  | KafkaFlow.Retry <br/>KafkaFlow.Retry.API<br/><br/>KafkaFlow.Retry.SqlServer<br/>or<br/>KafkaFlow.Retry.MongoDb | 

# Installing via NuGet
Install packages related to your context. The Core package is required for all other packages. 

## Requirements
**.NET Core 2.1 and later using Hosted Service**

## Packages

|Name                             |nuget.org|
|---------------------------------|----|
|KafkaFlow.Retry|[![Nuget Package](https://img.shields.io/nuget/v/KafkaFlow.Retry.svg?logo=nuget)](https://www.nuget.org/packages/KafkaFlow.Retry/) ![Nuget downloads](https://img.shields.io/nuget/dt/KafkaFlow.Retry.svg)
|KafkaFlow.Retry.API|[![Nuget Package](https://img.shields.io/nuget/v/KafkaFlow.Retry.API.svg?logo=nuget)](https://www.nuget.org/packages/KafkaFlow.Retry.API/) ![Nuget downloads](https://img.shields.io/nuget/dt/KafkaFlow.Retry.API.svg)
|KafkaFlow.Retry.MongoDb|[![Nuget Package](https://img.shields.io/nuget/v/KafkaFlow.Retry.MongoDb.svg?logo=nuget)](https://www.nuget.org/packages/KafkaFlow.Retry.MongoDb/) ![Nuget downloads](https://img.shields.io/nuget/dt/KafkaFlow.Retry.MongoDb.svg)
|KafkaFlow.Retry.SqlServer|[![Nuget Package](https://img.shields.io/nuget/v/KafkaFlow.Retry.SqlServer.svg?logo=nuget)](https://www.nuget.org/packages/KafkaFlow.Retry.SqlServer/) ![Nuget downloads](https://img.shields.io/nuget/dt/KafkaFlow.Retry.SqlServer.svg)

## Core package 
    Install-Package KafkaFlow.Retry

## HTTP API package
    Install-Package KafkaFlow.Retry.API

## MongoDb package 
    Install-Package KafkaFlow.Retry.MongoDb

## SqlServer package
    Install-Package KafkaFlow.Retry.SqlServer

# Usage &ndash; Simple and Forever retries policies
### Simple

```csharp
 .AddMiddlewares(
    middlewares => middlewares // KafkaFlow middlewares
    .RetrySimple(
        (config) => config
            .Handle<ExternalGatewayException>() // Exceptions to be handled
            .TryTimes(3)
            .WithTimeBetweenTriesPlan((retryCount) => 
                TimeSpan.FromMilliseconds(Math.Pow(2, retryCount)*1000) // exponential backoff
            )
    )
```

### Forever

```csharp
 .AddMiddlewares( 
    middlewares => middlewares // KafkaFlow middlewares
    .RetryForever(
        (config) => config
            .Handle<DatabaseTimeoutException>() // Exceptions to be handled
            .WithTimeBetweenTriesPlan(
                TimeSpan.FromMilliseconds(500),
                TimeSpan.FromMilliseconds(1000)
            )
    )
 
```

# Usage &ndash; Durable retry policy
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
-   [Sérgio Ribeiro](https://github.com/sergioamribeiro)


## License

[MIT](LICENSE.md)
