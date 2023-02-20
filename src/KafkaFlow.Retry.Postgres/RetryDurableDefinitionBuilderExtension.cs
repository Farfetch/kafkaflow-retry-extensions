﻿namespace KafkaFlow.Retry.Postgres
{
    public static class RetryDurableDefinitionBuilderExtension
    {
        public static RetryDurableDefinitionBuilder WithSqlServerDataProvider(
            this RetryDurableDefinitionBuilder retryDurableDefinitionBuilder,
            string connectionString,
            string databaseName)
        {
            retryDurableDefinitionBuilder.WithRepositoryProvider(
                new SqlServerDbDataProviderFactory()
                    .Create(
                        new SqlServerDbSettings(
                            connectionString,
                            databaseName)
                    )
                );

            return retryDurableDefinitionBuilder;
        }
    }
}