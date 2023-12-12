using System.Diagnostics.CodeAnalysis;

namespace KafkaFlow.Retry.SqlServer.Model;

[ExcludeFromCodeCoverage]
internal class RetryQueueItemMessageHeaderDbo
{
    public long Id { get; set; }

    public string Key { get; set; }

    public long RetryQueueItemMessageId { get; set; }

    public byte[] Value { get; set; }
}