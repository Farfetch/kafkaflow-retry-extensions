namespace KafkaFlow.Retry.Postgres
{
    using System.Diagnostics.CodeAnalysis;
    using Dawn;
    
    [ExcludeFromCodeCoverage]
    public class PostgresDbSettings
    {
        public PostgresDbSettings(string connectionString, string databaseName)
        {
            Guard.Argument(connectionString).NotNull().NotEmpty();
            Guard.Argument(databaseName).NotNull().NotEmpty();

            ConnectionString = connectionString;
            DatabaseName = databaseName;
        }

        public string ConnectionString { get; }

        public string DatabaseName { get; }
    }
}
