using Microsoft.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using Dawn;

namespace KafkaFlow.Retry.SqlServer;

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

    public string Schema => sqlServerDbSettings.Schema;

    public void Commit()
    {
        if (sqlTransaction is object)
        {
            sqlTransaction.Commit();
            committed = true;
        }
    }

    public SqlCommand CreateCommand()
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

    private SqlConnection GetDbConnection()
    {
        if (sqlConnection is null)
        {
            sqlConnection = new SqlConnection(sqlServerDbSettings.ConnectionString);
            sqlConnection.Open();
            sqlConnection.ChangeDatabase(sqlServerDbSettings.DatabaseName);
        }
        return sqlConnection;
    }

    private SqlTransaction GetDbTransaction()
    {
        if (sqlTransaction is null)
        {
            sqlTransaction = GetDbConnection().BeginTransaction();
        }
        return sqlTransaction;
    }
}