using Microsoft.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using Dawn;

namespace KafkaFlow.Retry.SqlServer;

[ExcludeFromCodeCoverage]
internal sealed class DbConnectionContext : IDbConnectionWithinTransaction
{
    private readonly SqlServerDbSettings _sqlServerDbSettings;
    private readonly bool _withinTransaction;
    private bool _committed = false;
    private SqlConnection _sqlConnection;
    private SqlTransaction _sqlTransaction;

    public DbConnectionContext(SqlServerDbSettings sqlServerDbSettings, bool withinTransaction)
    {
        Guard.Argument(sqlServerDbSettings).NotNull();
        _sqlServerDbSettings = sqlServerDbSettings;
        _withinTransaction = withinTransaction;
    }

    public string Schema => _sqlServerDbSettings.Schema;

    public void Commit()
    {
        if (_sqlTransaction is object)
        {
            _sqlTransaction.Commit();
            _committed = true;
        }
    }

    public SqlCommand CreateCommand()
    {
        var dbCommand = GetDbConnection().CreateCommand();

        if (_withinTransaction)
        {
            dbCommand.Transaction = GetDbTransaction();
        }

        return dbCommand;
    }

    public void Dispose()
    {
        if (_sqlTransaction is object)
        {
            if (!_committed)
            {
                Rollback();
            }
            _sqlTransaction.Dispose();
        }

        if (_sqlConnection is object)
        {
            _sqlConnection.Dispose();
        }
    }

    public void Rollback()
    {
        if (_sqlTransaction is object)
        {
            _sqlTransaction.Rollback();
        }
    }

    private SqlConnection GetDbConnection()
    {
        if (_sqlConnection is null)
        {
            _sqlConnection = new SqlConnection(_sqlServerDbSettings.ConnectionString);
            _sqlConnection.Open();
            _sqlConnection.ChangeDatabase(_sqlServerDbSettings.DatabaseName);
        }
        return _sqlConnection;
    }

    private SqlTransaction GetDbTransaction()
    {
        if (_sqlTransaction is null)
        {
            _sqlTransaction = GetDbConnection().BeginTransaction();
        }
        return _sqlTransaction;
    }
}