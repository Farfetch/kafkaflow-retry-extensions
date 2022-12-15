---
sidebar_position: 2
---

# Forever Retries

In this section, we will learn how to use Forever Retries.

If you face semi-transient faults that may self-correct after multiple retries, you may consider retrying forever.

The configuration can be done during [KafkaFlow Configuration](https://farfetch.github.io/kafkaflow/docs/guides/configuration) process by registering a [Middleware](https://farfetch.github.io/kafkaflow/docs/guides/middlewares/).

## How to use it

Install the [KafkaFlow.Retry](https://www.nuget.org/packages/KafkaFlow.Retry) package. 

```bash
dotnet add package KafkaFlow.Retry
```

On the configuration, add the `RetryForever` middleware extension method to your consumer middlewares to use it. 

The `RetryForever` receives an Action as an argument to configure the Retry policy. 
 

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
    ...
)
```


## How to define the Exception to be Handled

Exception Handling configurations can be found [here](exception-handling).