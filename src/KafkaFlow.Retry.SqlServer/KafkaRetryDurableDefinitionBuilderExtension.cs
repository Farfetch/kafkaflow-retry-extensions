namespace KafkaFlow.Retry.SqlServer
{
    public static class KafkaRetryDurableDefinitionBuilderExtension
    {
        public static KafkaRetryDurableDefinitionBuilder WithSqlServerDataProvider(
            this KafkaRetryDurableDefinitionBuilder kafkaRetryDurableDefinitionBuilder,
            string connectionString,
            string databaseName)
        {
            kafkaRetryDurableDefinitionBuilder.WithDataProvider(
                new SqlServerDbDataProviderFactory()
                    .Create(
                        new SqlServerDbSettings(
                            connectionString,
                            databaseName)
                    )
                );

            return kafkaRetryDurableDefinitionBuilder;
        }
    }
}