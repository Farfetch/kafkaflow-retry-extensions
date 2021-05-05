namespace KafkaFlow.Retry.MongoDb
{
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; }

        public string DatabaseName { get; set; }

        public string RetryQueueCollectionName { get; set; }

        public string RetryQueueItemCollectionName { get; set; }
    }
}