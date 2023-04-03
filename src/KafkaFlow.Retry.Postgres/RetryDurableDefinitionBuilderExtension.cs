namespace KafkaFlow.Retry.Postgres
{
    public static class RetryDurableDefinitionBuilderExtension
    {
        public static RetryDurableDefinitionBuilder WithPostgresDataProvider(
            this RetryDurableDefinitionBuilder retryDurableDefinitionBuilder,
            string connectionString,
            string databaseName)
        {
            retryDurableDefinitionBuilder.WithRepositoryProvider(
                new PostgresDbDataProviderFactory()
                    .Create(
                        new PostgresDbSettings(
                            connectionString,
                            databaseName)
                    )
                );

            return retryDurableDefinitionBuilder;
        }
    }
}