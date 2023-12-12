using KafkaFlow.Retry.MongoDb.Model;
using MongoDB.Driver;

namespace KafkaFlow.Retry.MongoDb;
internal sealed class DbContext
{
    private readonly IMongoDatabase database;
    private readonly MongoDbSettings mongoDbSettings;

    public DbContext(MongoDbSettings mongoDbSettings, IMongoClient mongoClient)
    {
            this.mongoDbSettings = mongoDbSettings;
            MongoClient = mongoClient;

            database = mongoClient.GetDatabase(this.mongoDbSettings.DatabaseName);
        }

    public IMongoClient MongoClient { get; }
    public IMongoCollection<RetryQueueItemDbo> RetryQueueItems => database.GetCollection<RetryQueueItemDbo>(mongoDbSettings.RetryQueueItemCollectionName);

    public IMongoCollection<RetryQueueDbo> RetryQueues => database.GetCollection<RetryQueueDbo>(mongoDbSettings.RetryQueueCollectionName);
}