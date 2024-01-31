using System.Diagnostics.CodeAnalysis;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;

namespace KafkaFlow.Retry.MongoDb.Model;

[ExcludeFromCodeCoverage]
internal static class DboConfigurations
{
    internal static void TryAddIndexes(DbContext dbContext)
    {
        dbContext.RetryQueues.Indexes.CreateMany(
            new CreateIndexModel<RetryQueueDbo>[]
            {
                new(
                    Builders<RetryQueueDbo>.IndexKeys.Ascending(x => x.SearchGroupKey)
                ),
                new(
                    Builders<RetryQueueDbo>.IndexKeys.Ascending(x => x.QueueGroupKey),
                    new CreateIndexOptions { Unique = true }
                ),
                new(
                    Builders<RetryQueueDbo>.IndexKeys.Ascending(x => x.Status)
                ),
                new(
                    Builders<RetryQueueDbo>.IndexKeys.Descending(x => x.CreationDate)
                ),
                new(
                    Builders<RetryQueueDbo>.IndexKeys.Ascending(x => x.LastExecution)
                )
            }
        );

        dbContext.RetryQueueItems.Indexes.CreateMany(
            new CreateIndexModel<RetryQueueItemDbo>[]
            {
                new(
                    Builders<RetryQueueItemDbo>.IndexKeys.Ascending(x => x.RetryQueueId)
                ),
                new(
                    Builders<RetryQueueItemDbo>.IndexKeys.Ascending(x => x.Status)
                ),
                new(
                    Builders<RetryQueueItemDbo>.IndexKeys.Descending(x => x.SeverityLevel)
                ),
                new(
                    Builders<RetryQueueItemDbo>.IndexKeys.Ascending(x => x.Sort)
                )
            }
        );
    }

    internal static void TryRegisterClassMapppings()
    {
        BsonClassMap.TryRegisterClassMap<RetryQueueDbo>(cm =>
        {
            cm.AutoMap();
            cm.MapIdProperty(q => q.Id).SetIdGenerator(new GuidGenerator());
        });

        BsonClassMap.TryRegisterClassMap<RetryQueueItemDbo>(cm =>
        {
            cm.AutoMap();
            cm.MapIdProperty(q => q.Id).SetIdGenerator(new GuidGenerator());
        });
    }
}