using System.Diagnostics.CodeAnalysis;

namespace KafkaFlow.Retry.MongoDb.Model;

[ExcludeFromCodeCoverage]
public class RetryQueueHeaderDbo
{
    public string Key { get; set; }

    public byte[] Value { get; set; }
}