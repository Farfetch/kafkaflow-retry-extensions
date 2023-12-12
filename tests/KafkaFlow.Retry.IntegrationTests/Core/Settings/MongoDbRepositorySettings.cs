namespace KafkaFlow.Retry.IntegrationTests.Core.Settings;

internal class MongoDbRepositorySettings
{
    public string ConnectionString { get; set; }

    public string DatabaseName { get; set; }

    public string RetryQueueCollectionName { get; set; }

    public string RetryQueueItemCollectionName { get; set; }
}