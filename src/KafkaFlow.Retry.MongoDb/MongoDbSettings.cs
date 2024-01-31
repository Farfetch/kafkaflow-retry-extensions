using System.Diagnostics.CodeAnalysis;

namespace KafkaFlow.Retry.MongoDb;

[ExcludeFromCodeCoverage]
public class MongoDbSettings
{
    public string ConnectionString { get; set; }

    public string DatabaseName { get; set; }

    public string RetryQueueCollectionName { get; set; }

    public string RetryQueueItemCollectionName { get; set; }
}