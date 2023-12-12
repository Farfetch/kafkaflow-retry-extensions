using System.Diagnostics.CodeAnalysis;
using Dawn;
using Npgsql;

namespace KafkaFlow.Retry.Postgres;

[ExcludeFromCodeCoverage]
internal sealed class DbConnectionContext : IDbConnectionWithinTransaction
{
    private readonly PostgresDbSettings postgresDbSettings;
    private readonly bool withinTransaction;
    private bool committed;
    private NpgsqlConnection sqlConnection;
    private NpgsqlTransaction sqlTransaction;

    public DbConnectionContext(PostgresDbSettings postgresDbSettings, bool withinTransaction)
    {
            Guard.Argument(postgresDbSettings).NotNull();
            this.postgresDbSettings = postgresDbSettings;
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

    public NpgsqlCommand CreateCommand()
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

    private NpgsqlConnection GetDbConnection()
    {
            if (this.sqlConnection is null)
            {
                this.sqlConnection = new NpgsqlConnection(this.postgresDbSettings.ConnectionString);
                this.sqlConnection.Open();
                this.sqlConnection.ChangeDatabase(this.postgresDbSettings.DatabaseName);
            }
            return this.sqlConnection;
        }

    private NpgsqlTransaction GetDbTransaction()
    {
            if (this.sqlTransaction is null)
            {
                this.sqlTransaction = this.GetDbConnection().BeginTransaction();
            }
            return this.sqlTransaction;
        }
}