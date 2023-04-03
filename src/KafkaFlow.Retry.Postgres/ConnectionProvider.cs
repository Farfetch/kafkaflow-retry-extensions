namespace KafkaFlow.Retry.Postgres
{
    using Dawn;

    internal sealed class ConnectionProvider : IConnectionProvider
    {
        public IDbConnection Create(PostgresDbSettings postgresDbSettings)
        {
            Guard.Argument(postgresDbSettings).NotNull();

            return new DbConnectionContext(postgresDbSettings, false);
        }

        public IDbConnectionWithinTransaction CreateWithinTransaction(PostgresDbSettings postgresDbSettings)
        {
            Guard.Argument(postgresDbSettings).NotNull();

            return new DbConnectionContext(postgresDbSettings, true);
        }
    }
}