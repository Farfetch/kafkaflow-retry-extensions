---
sidebar_position: 1
---

# Simple Retries

In this section, we will learn how to use Simple Retries.

The configuration can be done during [KafkaFlow Configuration](https://farfetch.github.io/kafkaflow/docs/guides/configuration) process by registering a [Middleware](https://farfetch.github.io/kafkaflow/docs/guides/middlewares/).

Simple Retries are useful since many faults are transient and may self-correct after a short delay.

## How to use it

Install the [KafkaFlow.Retry](https://www.nuget.org/packages/KafkaFlow.Retry) package. 

```bash
dotnet add package KafkaFlow.Retry
```

On the configuration, add the `RetrySimple` middleware extension method to your consumer middlewares to use it. 

The `RetrySimple` receives an Action as an argument to configure the Retry policy. 
 

```csharp
.AddMiddlewares(
    middlewares => middlewares // KafkaFlow middlewares
    .RetrySimple(
        (config) => config
            .Handle<ExternalGatewayException>() // Exceptions to be handled
            .TryTimes(3)
            .WithTimeBetweenTriesPlan((retryCount) => 
                TimeSpan.FromMilliseconds(Math.Pow(2, retryCount) * 1000) // exponential backoff
            )
    )
    ...
)
```

## Pausing the consumer when an Exception happens

You can configure the Policy to Stop the Consumer when the exception happens. You can configure it using the method `ShouldPauseConsumer`.

```csharp
.AddMiddlewares(
    middlewares => middlewares
    .RetrySimple(
        (config) => config
            ...
            .ShouldPauseConsumer(true)
            ...
    )
    ...
)

```

## How to define the Exception to be Handled

Exception Handling configurations can be found [here](exception-handling).
