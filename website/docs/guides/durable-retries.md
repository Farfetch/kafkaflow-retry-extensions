---
sidebar_position: 3
---

# Durable Retries

In this section, we will learn how to use Durable Retries.

Durable Retries are useful when beyond a certain amount of retries and waiting, you want to keep processing next-in-line messages but you can't lose the current offset message. 
As persistence databases, MongoDb or SqlServer is available. And you can manage in-retry messages through HTTP API.    

The configuration can be done during [KafkaFlow Configuration](https://farfetch.github.io/kafkaflow/docs/guides/configuration) process by registering a [Middleware](https://farfetch.github.io/kafkaflow/docs/guides/middlewares/).

## How to use it

Install the [KafkaFlow.Retry](https://www.nuget.org/packages/KafkaFlow.Retry) package. 

```bash
dotnet add package KafkaFlow.Retry
```

Install the package for the desired storage:
 - SqlServer: [KafkaFlow.Retry.SqlServer](https://www.nuget.org/packages/KafkaFlow.Retry.SqlServer) 
 - MongoDb: [KafkaFlow.Retry.MongoDb](https://www.nuget.org/packages/KafkaFlow.Retry.MongoDb) 


On the configuration, add the `RetryDurable` middleware extension method to your consumer middlewares to use it. 

The `RetryDurable` receives an Action as an argument to configure the Retry policy. 
 

```csharp
.AddMiddlewares(
    middlewares => middlewares // KafkaFlow middlewares
    .RetryDurable(
        (config) => config
            .Handle<NonBlockingException>()
            .WithMessageType(typeof(OrderMessage)) // Message type to be consumed
            .WithEmbeddedRetryCluster( // Retry consumer config
                cluster,
                config => config
                    .WithRetryTopicName("order-topic-retry")
                    .WithRetryConsumerBufferSize(4)
                    .WithRetryConsumerWorkersCount(2)
                    .WithRetryConsumerStrategy(RetryConsumerStrategy.GuaranteeOrderedConsumption)
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
            
            .WithMongoDbDataProvider(...)
            // OR
            .WithSqlServerDataProvider(...)

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
    ...
)
```

:::info
As you can see above, there's a retry plan configured (`WithRetryPlanBeforeRetryDurable`) to execute before the Durable Plan.
It's a Simple Retry that can perform a given number of attempts before delegating to the Durable policy.
:::

:::note
You can find other samples [here](https://github.com/Farfetch/kafkaflow-retry-extensions/tree/main/samples).
:::

## How to configure Message Type and Serialization

Durable Retries require the definition of a Message Type. It relies on [KafkaFlow Serializer Middleware](https://farfetch.github.io/kafkaflow/docs/guides/middlewares/serializer-middleware/) to perform the serialization.

You can find here an example using Avro.

```csharp
.AddMiddlewares(
    middlewares => middlewares
    .AddSchemaRegistryAvroSerializer()
    .RetryDurable(
        (config) => config
            .Handle<RetryDurableTestException>()
            .WithMessageType(typeof(AvroLogMessage))
            .WithMessageSerializeSettings(new JsonSerializerSettings
            {
                ContractResolver = new WritablePropertiesOnlyResolver()
            })
            ...
    )
    ...
)
```

## How to use MongoDb as a Provider

Install the [KafkaFlow.Retry.MongoDb](https://www.nuget.org/packages/KafkaFlow.Retry.MongoDb) package. 

```bash
dotnet add package KafkaFlow.Retry.MongoDb
```

On the configuration, define the access configuration to the MongoDb instance.

```csharp
.AddMiddlewares(
    middlewares => middlewares 
    .RetryDurable(
        (config) => config
            ...
            .WithMongoDbDataProvider(
                connectionString,
                database,
                retryQueueCollectionName,
                retryQueueItemCollectionName)
            ...
    )
    ...
)
```


## How to use SQL Server as a Provider

Install the [KafkaFlow.Retry.SqlServer](https://www.nuget.org/packages/KafkaFlow.Retry.SqlServer) package. 

```bash
dotnet add package KafkaFlow.Retry.SqlServer
```

On the configuration, define the access configuration to the SqlServer instance.

```csharp
.AddMiddlewares(
    middlewares => middlewares 
    .RetryDurable(
        (config) => config
            ...
            .WithSqlServerDataProvider(
                connectionString,
                databaseName)
            ...
    )
    ...
)
```

## How to configure an HTTP API to manage the Data Provider

Install the [KafkaFlow.Retry.API](https://www.nuget.org/packages/KafkaFlow.Retry.API) package. 

```bash
dotnet add package KafkaFlow.Retry.API
```

Once you install the Package, install also the package for the desired storage:
 - SqlServer: [KafkaFlow.Retry.SqlServer](https://www.nuget.org/packages/KafkaFlow.Retry.SqlServer) 
 - MongoDb: [KafkaFlow.Retry.MongoDb](https://www.nuget.org/packages/KafkaFlow.Retry.MongoDb) 


:::note
You can find a sample [here](https://github.com/Farfetch/kafkaflow-retry-extensions/tree/main/samples/KafkaFlow.Retry.API.Sample).
:::
