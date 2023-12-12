namespace KafkaFlow.Retry.Postgres;

internal interface IConnectionProvider
{
    IDbConnection Create(PostgresDbSettings postgresDbSettings);

    IDbConnectionWithinTransaction CreateWithinTransaction(PostgresDbSettings postgresDbSettings);
}