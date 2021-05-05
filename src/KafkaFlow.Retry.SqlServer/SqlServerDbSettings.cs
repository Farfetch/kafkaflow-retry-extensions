namespace KafkaFlow.Retry.SqlServer
{
    using Dawn;
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    public class SqlServerDbSettings
    {
        public SqlServerDbSettings(string connectionString, string databaseName)
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
