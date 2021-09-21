namespace KafkaFlow.Retry.MongoDb.Model
{
    using System.Diagnostics.CodeAnalysis;
    using MongoDB.Bson.Serialization;
    using MongoDB.Bson.Serialization.IdGenerators;
    using MongoDB.Driver;

    [ExcludeFromCodeCoverage]
    internal static class DboConfigurations
    {
        internal static void TryAddIndexes(DbContext dbContext)
        {
            dbContext.RetryQueues.Indexes.CreateMany(
                new CreateIndexModel<RetryQueueDbo>[]
                {
                    new CreateIndexModel<RetryQueueDbo>(
                        Builders<RetryQueueDbo>.IndexKeys.Ascending(x => x.SearchGroupKey)
                    ),
                    new CreateIndexModel<RetryQueueDbo>(
                        Builders<RetryQueueDbo>.IndexKeys.Ascending(x => x.QueueGroupKey),
                        new CreateIndexOptions{ Unique = true }
                    ),
                    new CreateIndexModel<RetryQueueDbo>(
                        Builders<RetryQueueDbo>.IndexKeys.Ascending(x => x.Status)
                    ),
                     new CreateIndexModel<RetryQueueDbo>(
                        Builders<RetryQueueDbo>.IndexKeys.Descending(x => x.CreationDate)
                    ),
                      new CreateIndexModel<RetryQueueDbo>(
                        Builders<RetryQueueDbo>.IndexKeys.Ascending(x => x.LastExecution)
                    )
                }
            );

            dbContext.RetryQueueItems.Indexes.CreateMany(
                new CreateIndexModel<RetryQueueItemDbo>[]
                {
                    new CreateIndexModel<RetryQueueItemDbo>(
                        Builders<RetryQueueItemDbo>.IndexKeys.Ascending(x => x.RetryQueueId)
                    ),
                    new CreateIndexModel<RetryQueueItemDbo>(
                        Builders<RetryQueueItemDbo>.IndexKeys.Ascending(x => x.Status)
                    ),
                    new CreateIndexModel<RetryQueueItemDbo>(
                        Builders<RetryQueueItemDbo>.IndexKeys.Descending(x => x.SeverityLevel)
                    ),
                    new CreateIndexModel<RetryQueueItemDbo>(
                        Builders<RetryQueueItemDbo>.IndexKeys.Ascending(x => x.Sort)
                    )
                }
            );
        }

        internal static void TryRegisterClassMapppings()
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(RetryQueueDbo)))
            {
                BsonClassMap.RegisterClassMap<RetryQueueDbo>(cm =>
                    {
                        cm.AutoMap();
                        cm.MapIdProperty(q => q.Id).SetIdGenerator(new GuidGenerator());
                    });
            }

            if (!BsonClassMap.IsClassMapRegistered(typeof(RetryQueueItemDbo)))
            {
                BsonClassMap.RegisterClassMap<RetryQueueItemDbo>(cm =>
                    {
                        cm.AutoMap();
                        cm.MapIdProperty(q => q.Id).SetIdGenerator(new GuidGenerator());
                    });
            }
        }
    }
}