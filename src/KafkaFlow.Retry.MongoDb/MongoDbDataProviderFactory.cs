using System;
using Dawn;
using KafkaFlow.Retry.Durable.Repository;
using KafkaFlow.Retry.MongoDb.Model;
using KafkaFlow.Retry.MongoDb.Repositories;
using MongoDB.Driver;

namespace KafkaFlow.Retry.MongoDb;

public sealed class MongoDbDataProviderFactory
{
    public MongoDbDataProviderFactory()
    {
        DboConfigurations.TryRegisterClassMapppings();
    }

    public DataProviderCreationResult TryCreate(MongoDbSettings mongoDbSettings)
    {
        Guard.Argument(mongoDbSettings)
            .NotNull(
                $"It is mandatory to configure the factory before creating new instances of {nameof(IRetryDurableQueueRepositoryProvider)}. Make sure the Config method is executed before the Create method.");
        try
        {
            var mongoClient = new MongoClient(mongoDbSettings.ConnectionString);
            var dbContext = new DbContext(mongoDbSettings, mongoClient);
            DboConfigurations.TryAddIndexes(dbContext);
            return new DataProviderCreationResult(
                null,
                new RetryQueueDataProvider(
                    dbContext,
                    new RetryQueueRepository(dbContext),
                    new RetryQueueItemRepository(dbContext)),
                true);
        }
        catch (Exception ex)
        {
            return new DataProviderCreationResult(ex.ToString(), null, false);
        }
    }
}