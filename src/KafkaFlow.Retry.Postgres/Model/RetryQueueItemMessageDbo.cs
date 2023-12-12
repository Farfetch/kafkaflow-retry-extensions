using System;
using System.Diagnostics.CodeAnalysis;

namespace KafkaFlow.Retry.Postgres.Model;

[ExcludeFromCodeCoverage]
internal class RetryQueueItemMessageDbo
{
    public long IdRetryQueueItem { get; set; }

    public byte[] Key { get; set; }

    public long Offset { get; set; }

    public int Partition { get; set; }

    public string TopicName { get; set; }

    public DateTime UtcTimeStamp { get; set; }

    public byte[] Value { get; set; }
}