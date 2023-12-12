using System.Diagnostics.CodeAnalysis;
using Dawn;

namespace KafkaFlow.Retry.SqlServer;

/// <summary>
/// Defines the Sql Server database settings.
/// </summary>

[ExcludeFromCodeCoverage]
public class SqlServerDbSettings
{
    private const string schemaDefault = "dbo";

    /// <summary>
    /// Creates a Sql Server database settings with schema
    /// </summary>
    /// <param name="connectionString">The connection string of the Sql Server.</param>
    /// <param name="databaseName">The database name.</param>
    /// <param name="schema">The schema name.</param>
    public SqlServerDbSettings(string connectionString, string databaseName, string schema)
    {
            Guard.Argument(connectionString).NotNull().NotEmpty();
            Guard.Argument(databaseName).NotNull().NotEmpty();
            Guard.Argument(schema).NotNull().NotEmpty();

            ConnectionString = connectionString;
            DatabaseName = databaseName;
            Schema = schema;
        }

    /// <summary>
    /// Creates a Sql Server database settings
    /// </summary>
    /// <param name="connectionString">The connection string of the Sql Server.</param>
    /// <param name="databaseName">The database name.</param>

    public SqlServerDbSettings(string connectionString, string databaseName)
    {
            Guard.Argument(connectionString).NotNull().NotEmpty();
            Guard.Argument(databaseName).NotNull().NotEmpty();

            ConnectionString = connectionString;
            DatabaseName = databaseName;
            Schema = schemaDefault;
        }

    /// <summary>
    /// Gets the connection string of the Sql Server database.
    /// </summary>
    public string ConnectionString { get; }

    /// <summary>
    /// Gets the database name.
    /// </summary>
    public string DatabaseName { get; }

    /// <summary>
    /// Gets the schema name.
    /// </summary>
    public string Schema { get; }
}