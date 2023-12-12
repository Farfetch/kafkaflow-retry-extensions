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
            if (sqlTransaction is object)
            {
                sqlTransaction.Commit();
                committed = true;
            }
        }

    public NpgsqlCommand CreateCommand()
    {
            var dbCommand = GetDbConnection().CreateCommand();

            if (withinTransaction)
            {
                dbCommand.Transaction = GetDbTransaction();
            }

            return dbCommand;
        }

    public void Dispose()
    {
            if (sqlTransaction is object)
            {
                if (!committed)
                {
                    Rollback();
                }
                sqlTransaction.Dispose();
            }

            if (sqlConnection is object)
            {
                sqlConnection.Dispose();
            }
        }

    public void Rollback()
    {
            if (sqlTransaction is object)
            {
                sqlTransaction.Rollback();
            }
        }

    private NpgsqlConnection GetDbConnection()
    {
            if (sqlConnection is null)
            {
                sqlConnection = new NpgsqlConnection(postgresDbSettings.ConnectionString);
                sqlConnection.Open();
                sqlConnection.ChangeDatabase(postgresDbSettings.DatabaseName);
            }
            return sqlConnection;
        }

    private NpgsqlTransaction GetDbTransaction()
    {
            if (sqlTransaction is null)
            {
                sqlTransaction = GetDbConnection().BeginTransaction();
            }
            return sqlTransaction;
        }
}