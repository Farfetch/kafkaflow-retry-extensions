using System;
using System.Diagnostics.CodeAnalysis;
using KafkaFlow.Retry.Durable.Common;
using KafkaFlow.Retry.Durable.Repository.Model;

namespace KafkaFlow.Retry.SqlServer.Model;

[ExcludeFromCodeCoverage]
internal class RetryQueueItemDbo
{
    public int AttemptsCount { get; set; }

    public DateTime CreationDate { get; set; }

    public string Description { get; set; }

    public Guid DomainRetryQueueId { get; set; }

    public long Id { get; set; }

    public Guid IdDomain { get; set; }

    public DateTime? LastExecution { get; set; }

    public DateTime? ModifiedStatusDate { get; set; }

    public long RetryQueueId { get; set; }

    public SeverityLevel SeverityLevel { get; set; }

    public int Sort { get; set; }

    public RetryQueueItemStatus Status { get; set; }
}