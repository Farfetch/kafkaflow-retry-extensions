namespace KafkaFlow.Retry.SqlServer
{
    using KafkaFlow.Retry.MongoDb;

    public static class KafkaRetryDurableDefinitionBuilderExtension
    {
        public static KafkaRetryDurableDefinitionBuilder WithMongoDbDataProvider(
            this KafkaRetryDurableDefinitionBuilder kafkaRetryDurableDefinitionBuilder,
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

            kafkaRetryDurableDefinitionBuilder.WithDataProvider(dataProviderCreation.Result);

            return kafkaRetryDurableDefinitionBuilder;
        }
    }
}