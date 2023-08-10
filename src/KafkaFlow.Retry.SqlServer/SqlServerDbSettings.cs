namespace KafkaFlow.Retry.SqlServer
{
    using System.Diagnostics.CodeAnalysis;
    using Dawn;

    /// <summary>
    /// Defines the Sql Server database settings.
    /// </summary>

    [ExcludeFromCodeCoverage]
    public class SqlServerDbSettings
    {
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
        }

        /// <summary>
        /// Gets the connection string of the Sql Server database.
        /// </summary>
        public string ConnectionString { get; }

        /// <summary>
        /// Gets the database name.
        /// </summary>
        public string DatabaseName { get; }
    }
}