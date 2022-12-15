---
sidebar_position: 1
---


# Installation

KafkaFlow Retry Extensions is a set of NuGet packages that can extend [KafkaFlow](https://github.com/Farfetch/kafkaflow).


## Prerequisites

 - One of the following .NET versions
   - .NET Core 2.1 or above.
   - .NET Framework 4.6.1 or above.
 - KafkaFlow NuGet package installed.


## Installing

On an application with KafkaFlow configured, install KafkaFlow Retry Extensions using NuGet package management.

Required Package:

* [KafkaFlow.Retry](https://www.nuget.org/packages/KafkaFlow.Retry/)


You can quickly install it using .NET CLI 👇
```shell
dotnet add package KafkaFlow.Retry
```

You can find a complete list of the available packages [here](packages).

## Setup

Types are in the `KafkaFlow.Retry` namespace.

```csharp
using KafkaFlow;
using KafkaFlow.Retry;
```

The Retry Extensions library exposes a middleware. To use them, edit your KafkaFlow configuration add register a new middleware, as you can see below.

```csharp

.AddMiddlewares(
    middlewares => middlewares // KafkaFlow middlewares
    .RetrySimple(
        (config) => config
            .Handle<ExternalGatewayException>() // Exception to be handled
            .TryTimes(3)
            .WithTimeBetweenTriesPlan((retryCount) => 
                TimeSpan.FromMilliseconds(Math.Pow(2, retryCount)*1000) // exponential backoff
            )
    )
```

You can use other types of retry policies such as [Forever](../guides/forever-retries) or [Durable](../guides/durable-retries).


