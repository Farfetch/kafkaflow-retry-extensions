namespace KafkaFlow.Retry.SqlServer
{
    using System.Diagnostics.CodeAnalysis;
    using Dawn;

    [ExcludeFromCodeCoverage]
    public class SqlServerDbSettings
    {
        public SqlServerDbSettings(string connectionString, string databaseName, string schema = "dbo")
        {
            Guard.Argument(connectionString).NotNull().NotEmpty();
            Guard.Argument(databaseName).NotNull().NotEmpty();

            ConnectionString = connectionString;
            DatabaseName = databaseName;
            Schema = schema;
        }

        public string ConnectionString { get; }

        public string DatabaseName { get; }

        public string Schema { get; }
    }
}