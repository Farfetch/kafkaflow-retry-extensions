namespace KafkaFlow.Retry.SqlServer
{
    using System.Data.SqlClient;
    using System.Diagnostics.CodeAnalysis;
    using Dawn;

    [ExcludeFromCodeCoverage]
    internal sealed class DbConnectionContext : IDbConnectionWithinTransaction
    {
        private readonly SqlServerDbSettings sqlServerDbSettings;
        private readonly bool withinTransaction;
        private bool committed = false;
        private SqlConnection sqlConnection;
        private SqlTransaction sqlTransaction;

        public DbConnectionContext(SqlServerDbSettings sqlServerDbSettings, bool withinTransaction)
        {
            Guard.Argument(sqlServerDbSettings).NotNull();
            this.sqlServerDbSettings = sqlServerDbSettings;
            this.withinTransaction = withinTransaction;
        }

        public void Commit()
        {
            if (this.sqlTransaction is object)
            {
                this.sqlTransaction.Commit();
                this.committed = true;
            }
        }

        public SqlCommand CreateCommand()
        {
            var dbCommand = this.GetDbConnection().CreateCommand();

            if (this.withinTransaction)
            {
                dbCommand.Transaction = this.GetDbTransaction();
            }

            return dbCommand;
        }

        public void Dispose()
        {
            if (this.sqlTransaction is object)
            {
                if (!this.committed)
                {
                    this.Rollback();
                }
                this.sqlTransaction.Dispose();
            }

            if (this.sqlConnection is object)
            {
                this.sqlConnection.Dispose();
            }
        }

        public void Rollback()
        {
            if (this.sqlTransaction is object)
            {
                this.sqlTransaction.Rollback();
            }
        }

        private SqlConnection GetDbConnection()
        {
            if (this.sqlConnection is null)
            {
                this.sqlConnection = new SqlConnection(this.sqlServerDbSettings.ConnectionString);
                this.sqlConnection.Open();
                this.sqlConnection.ChangeDatabase(this.sqlServerDbSettings.DatabaseName);
            }
            return this.sqlConnection;
        }

        private SqlTransaction GetDbTransaction()
        {
            if (this.sqlTransaction is null)
            {
                this.sqlTransaction = this.GetDbConnection().BeginTransaction();
            }
            return this.sqlTransaction;
        }
    }
}