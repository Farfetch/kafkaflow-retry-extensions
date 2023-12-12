using System.Diagnostics.CodeAnalysis;
using Dawn;

namespace KafkaFlow.Retry.Postgres;

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