using KafkaFlow.Retry.MongoDb.Model;
using MongoDB.Driver;

namespace KafkaFlow.Retry.MongoDb;

internal sealed class DbContext
{
    private readonly IMongoDatabase _database;
    private readonly MongoDbSettings _mongoDbSettings;

    public DbContext(MongoDbSettings mongoDbSettings, IMongoClient mongoClient)
    {
        _mongoDbSettings = mongoDbSettings;
        MongoClient = mongoClient;

        _database = mongoClient.GetDatabase(_mongoDbSettings.DatabaseName);
    }

    public IMongoClient MongoClient { get; }

    public IMongoCollection<RetryQueueItemDbo> RetryQueueItems =>
        _database.GetCollection<RetryQueueItemDbo>(_mongoDbSettings.RetryQueueItemCollectionName);

    public IMongoCollection<RetryQueueDbo> RetryQueues =>
        _database.GetCollection<RetryQueueDbo>(_mongoDbSettings.RetryQueueCollectionName);
}