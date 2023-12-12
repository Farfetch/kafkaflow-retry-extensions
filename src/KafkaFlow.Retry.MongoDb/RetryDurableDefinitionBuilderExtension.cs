namespace KafkaFlow.Retry.MongoDb;

public static class RetryDurableDefinitionBuilderExtension
{
    public static RetryDurableDefinitionBuilder WithMongoDbDataProvider(
        this RetryDurableDefinitionBuilder retryDurableDefinitionBuilder,
        string connectionString,
        string databaseName,
        string mongoDbretryQueueCollectionName,
        string mongoDbretryQueueItemCollectionName)
    {
            var dataProviderCreation = new MongoDbDataProviderFactory()
                    .TryCreate(
                        new MongoDbSettings
                        {
                            ConnectionString = connectionString,
                            DatabaseName = databaseName,
                            RetryQueueCollectionName = mongoDbretryQueueCollectionName,
                            RetryQueueItemCollectionName = mongoDbretryQueueItemCollectionName
                        }
                    );

            if (!dataProviderCreation.Success)
            {
                throw new DataProviderCreationException($"The Retry Queue Data Provider could not be created. Error: {dataProviderCreation.Message}");
            }

            retryDurableDefinitionBuilder.WithRepositoryProvider(dataProviderCreation.Result);

            return retryDurableDefinitionBuilder;
        }
}