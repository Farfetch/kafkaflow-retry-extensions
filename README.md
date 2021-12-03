# KafkaFlow Retry Extensions
KafkaFlow Retry is a .NET framework to retry messages on consumers, simple to use.

KafkaFlow Retry is an extention of [Kafka Flow](https://github.com/Farfetch/kafka-flow).

## Resilience policies

|Policy| Description | Aka| Required Packages|
| ------------- | ------------- |:-------------: |------------- |
|**Simple Retry** <br/>(policy family)<br/><sub>([quickstart](#simple)&nbsp;;&nbsp;deep)</sub>|Many faults are transient and may self-correct after a short delay.| "Maybe it's just a blip" |   KafkaFlow.Retry |
|**Forever Retry**<br/>(policy family)<br/><sub>([quickstart](#forever)&nbsp;;&nbsp;deep)</sub>|Many faults are semi-transient and may self-correct after multiple retries. | "Never give up" | KafkaFlow.Retry | 
|**Durable Retry**<br/><sub>([quickstart](#durable)&nbsp;;&nbsp;deep)</sub>|Beyond a certain amount of retries and wait, you want to keep processing next-in-line messages but you can't loss the current offset message. As persistance databases, MongoDb or SqlServer are available. And you can manage in-retry messages through HTTP API.| "I can't stop processing messages but I can't loss messages"  | KafkaFlow.Retry <br/>KafkaFlow.Retry.API<br/><br/>KafkaFlow.Retry.SqlServer<br/>or<br/>KafkaFlow.Retry.MongoDb | 

## Installing via NuGet
Install packages related to your context. The Core package is required for all other packages. 

## Requirements
**.NET Core 2.1 and later using Hosted Service**

## Packages

|Name                             |nuget.org|
|---------------------------------|----|
[![Codacy Badge](https://api.codacy.com/project/badge/Grade/076e73c10ed441b4a78ce41767d302dc)](https://app.codacy.com/gh/Farfetch/kafka-flow-retry-extensions?utm_source=github.com&utm_medium=referral&utm_content=Farfetch/kafka-flow-retry-extensions&utm_campaign=Badge_Grade_Settings)
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

## Usage &ndash; Simple and Forever retries policies
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

## Usage &ndash; Durable retry policy

### Durable

```csharp
.AddMiddlewares( 
    middlewares => middlewares // KafkaFlow middlewares
    .RetryDurable(
            config => config
                .Handle<NonBlockingException>() // Exceptions to be handled
                .WithMessageType(typeof(TestMessage)) // Message type to be consumed
                .WithEmbeddedRetryCluster( // Retry consumer config
                    cluster,
                    config => config
                        .WithRetryTopicName("test-topic-retry")
                        .WithRetryConsumerBufferSize(4)
                        .WithRetryConsumerWorkersCount(2)
                        .WithRetryConusmerStrategy(RetryConsumerStrategy.GuaranteeOrderedConsumption)
                        .WithRetryTypedHandlers(
                            handlers => handlers
                                .WithHandlerLifetime(InstanceLifetime.Transient)
                                .AddHandler<Handler>()
                        ).Enabled(true)
                )
                .WithQueuePollingJobConfiguration( // Polling configuration
                    config => config
                        .WithId("custom_search_key")
                        .WithCronExpression("0 0/1 * 1/1 * ? *")
                        .WithExpirationIntervalFactor(1)
                        .WithFetchSize(10)
                        .Enabled(true)
                )                      
                .WithMongoDbDataProvider( // Persistence configuration
                    mongoDbconnectionString,
                    mongoDbdatabaseName,
                    mongoDbretryQueueCollectionName,
                    mongoDbretryQueueItemCollectionName
                )          
                .WithRetryPlanBeforeRetryDurable( // Chained simple retry before triggering durable 
                    config => config
                        .TryTimes(3)
                        .WithTimeBetweenTriesPlan(
                            TimeSpan.FromMilliseconds(250),
                            TimeSpan.FromMilliseconds(500),
                            TimeSpan.FromMilliseconds(1000))
                        .ShouldPauseConsumer(false)
                )
        )
    )
```

See the [setup page](https://github.com/Farfetch/kafka-flow-retry-extensions/wiki/Setup) and [samples](https://github.com/Farfetch/kafka-flow-retry-extensions/tree/main/samples) for more details

## Documentation

[Wiki Page](https://github.com/Farfetch/kafka-flow-retry-extensions/wiki)

## Contributing

Read the [Contributing guidelines](CONTRIBUTING.md)

## Maintainers

-   [Bruno Gomes](https://github.com/brunohfgomes)
-   [Carlos Goias](https://github.com/carlosgoias)
-   [Fernando Marins](https://github.com/fernando-a-marins)
-   [Luís Garcês](https://github.com/luispfgarces)
-   [Martinho Novais](https://github.com/martinhonovais)
-   [Rodrigo Belo](https://github.com/rodrigobelo)
-   [Sérgio Ribeiro](https://github.com/sergioamribeiro)


## License

[MIT](LICENSE.md)
