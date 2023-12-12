namespace KafkaFlow.Retry.SqlServer;

internal interface IConnectionProvider
{
    IDbConnection Create(SqlServerDbSettings sqlServerDbSettings);

    IDbConnectionWithinTransaction CreateWithinTransaction(SqlServerDbSettings sqlServerDbSettings);
}