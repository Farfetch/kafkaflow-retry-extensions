namespace KafkaFlow.Retry.Postgres
{
    internal interface IConnectionProvider
    {
        IDbConnection Create(SqlServerDbSettings sqlServerDbSettings);

        IDbConnectionWithinTransaction CreateWithinTransaction(SqlServerDbSettings sqlServerDbSettings);
    }
}