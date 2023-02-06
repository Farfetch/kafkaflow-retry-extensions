---
sidebar_position: 2
sidebar_label: Quickstart
---


# Quickstart: Apply your first Retry Policy

In this article, you use C# and the .NET CLI to create two applications that will produce and consume events from Apache Kafka.
The consumer will use a Simple Retry strategy to retry in case of a given exception type.

By the end of the article, you will know how to use KafkaFlow Retry Extensions to make your Consumers resilient.


## Prerequisites

 - [.NET 6.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
 - [Docker Desktop](https://www.docker.com/products/docker-desktop/)

## Overview

You will create two applications:

 1. **Consumer:** Will be running waiting for incoming messages and will write them to the console. The Message Handler will randomly fail, but the execution will retry.
 2. **Producer:** Will send a message every time you run the application.

To connect them, you will be running an Apache Kafka cluster using Docker.

## Steps

### 1. Create a folder for your applications

Create a new folder with the name _KafkaFlowRetryuickstart_.

### 2. Setup Apache Kafka

Inside the folder from step 1, create a `docker-compose.yml` file. You can download it from [here](https://github.com/Farfetch/kafkaflow/blob/master/docker-compose.yml).

### 3. Start the cluster

Using your terminal of choice, start the cluster.

```bash
docker-compose up -d
```

### 4. Create Producer Project

Run the following command to create a Console Project named _Producer_.
```bash
dotnet new console --name Producer
```

### 5. Install KafkaFlow packages

Inside the _Producer_ project directory, run the following commands to install the required packages.

```bash
dotnet add package KafkaFlow
dotnet add package KafkaFlow.Microsoft.DependencyInjection
dotnet add package KafkaFlow.LogHandler.Console
dotnet add package KafkaFlow.TypedHandler
dotnet add package KafkaFlow.Serializer
dotnet add package KafkaFlow.Serializer.JsonCore
dotnet add package Microsoft.Extensions.DependencyInjection
```

### 6. Create the Message contract

Add a new class file named _HelloMessage.cs_ and add the following example:

```csharp
namespace Producer;

public class HelloMessage
{
    public string Text { get; set; } = default!;
}
```

### 7. Create message sender

Replace the content of the _Program.cs_ with the following example:

```csharp
using Microsoft.Extensions.DependencyInjection;
using KafkaFlow.Producers;
using KafkaFlow.Serializer;
using KafkaFlow;
using Producer;

var services = new ServiceCollection();

const string topicName = "sample-topic";
const string producerName = "say-hello";

services.AddKafka(
    kafka => kafka
        .UseConsoleLog()
        .AddCluster(
            cluster => cluster
                .WithBrokers(new[] { "localhost:9092" })
                .CreateTopicIfNotExists(topicName, 1, 1)
                .AddProducer(
                    producerName,
                    producer => producer
                        .DefaultTopic(topicName)
                        .AddMiddlewares(m =>
                            m.AddSerializer<JsonCoreSerializer>()
                            )
                )
        )
);

var serviceProvider = services.BuildServiceProvider();

var producer = serviceProvider
    .GetRequiredService<IProducerAccessor>()
    .GetProducer(producerName);

await producer.ProduceAsync(
                   topicName,
                   Guid.NewGuid().ToString(),
                   new HelloMessage { Text = "Hello!" });


Console.WriteLine("Message sent!");

```


### 8. Create Consumer Project

Run the following command to create a Console Project named _Consumer_.
```bash
dotnet new console --name Consumer
```

### 9. Add a reference to the Producer

In order to access the message contract, add a reference to the Producer Project.

Inside the _Consumer_ project directory, run the following commands to add the reference.

```bash
dotnet add reference ../Producer
```

### 10. Install KafkaFlow packages

Inside the _Consumer_ project directory, run the following commands to install the required packages.

```bash
dotnet add package KafkaFlow
dotnet add package KafkaFlow.Microsoft.DependencyInjection
dotnet add package KafkaFlow.LogHandler.Console
dotnet add package KafkaFlow.TypedHandler
dotnet add package KafkaFlow.Serializer
dotnet add package KafkaFlow.Serializer.JsonCore
dotnet add package Microsoft.Extensions.DependencyInjection
```

### 11. Install KafkaFlow Retry Extensions packages

Inside the _Consumer_ project directory, run the following commands to install the required packages.

```bash
dotnet add package KafkaFlow.Retry
```

### 12. Create a Message Handler

Create a new class file named _HelloMessageHandler.cs_ and add the following example.

```csharp
using KafkaFlow;
using KafkaFlow.TypedHandler;
using Producer;

namespace Consumer;

public class HelloMessageHandler : IMessageHandler<HelloMessage>
{
    private static readonly Random Random = new(Guid.NewGuid().GetHashCode());
    private static bool ShouldFail() => Random.Next(2) == 1;

    public Task Handle(IMessageContext context, HelloMessage message)
    {
        if (ShouldFail())
        {
            Console.WriteLine(
                "Let's fail: Partition: {0} | Offset: {1} | Message: {2}",
                context.ConsumerContext.Partition,
                context.ConsumerContext.Offset,
                message.Text);
            throw new IOException();
        }

        Console.WriteLine(
            "Partition: {0} | Offset: {1} | Message: {2}",
            context.ConsumerContext.Partition,
            context.ConsumerContext.Offset,
            message.Text);

        return Task.CompletedTask;
    }
}
```

As you can see randomly the handler will throw an *IOException*.

### 13. Create the Message Consumer

Replace the content of the _Program.cs_ with the following example.

```csharp
using KafkaFlow;
using KafkaFlow.Serializer;
using Microsoft.Extensions.DependencyInjection;
using KafkaFlow.TypedHandler;
using KafkaFlow.Retry;
using Consumer;

const string topicName = "sample-topic";
var services = new ServiceCollection();

services.AddKafka(kafka => kafka
    .UseConsoleLog()
    .AddCluster(cluster => cluster
        .WithBrokers(new[] { "localhost:9092" })
        .CreateTopicIfNotExists(topicName, 1, 1)
        .AddConsumer(consumer => consumer
            .Topic(topicName)
            .WithAutoOffsetReset(AutoOffsetReset.Earliest)
            .WithGroupId("sample-group")
            .WithBufferSize(100)
            .WithWorkersCount(10)
            .AddMiddlewares(
                middlewares => middlewares
                    .RetrySimple(
                        (config) => config
                            .Handle<IOException>()
                            .TryTimes(3)
                            .WithTimeBetweenTriesPlan((retryCount) =>
                                TimeSpan.FromMilliseconds(Math.Pow(2, retryCount) * 1000)
                            )
                    )
                    .AddSerializer<JsonCoreSerializer>()
                    .AddTypedHandlers(h => h.AddHandler<HelloMessageHandler>())
            )
        )
    )
);

var serviceProvider = services.BuildServiceProvider();

var bus = serviceProvider.CreateKafkaBus();

await bus.StartAsync();

Console.ReadKey();

await bus.StopAsync();
```

### 14. Run!

From the `KafkaFlowRetryQuickstart` directory:

 1. Run the Consumer:
   
```bash
dotnet run --project Consumer/Consumer.csproj 
```

 2. From another terminal, run the Producer:

```bash
dotnet run --project Producer/Producer.csproj 
```