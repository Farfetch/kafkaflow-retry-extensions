using System;
using System.Diagnostics.CodeAnalysis;
using KafkaFlow.Retry.Durable.Repository.Model;

namespace KafkaFlow.Retry.MongoDb.Model;

[ExcludeFromCodeCoverage]
public class RetryQueueDbo
{
    public DateTime CreationDate { get; set; }

    public Guid Id { get; set; }

    public DateTime LastExecution { get; set; }

    public string QueueGroupKey { get; set; }

    public string SearchGroupKey { get; set; }

    public RetryQueueStatus Status { get; set; }
}