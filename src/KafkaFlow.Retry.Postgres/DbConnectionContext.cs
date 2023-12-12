using System.Diagnostics.CodeAnalysis;
using Dawn;
using Npgsql;

namespace KafkaFlow.Retry.Postgres;

[ExcludeFromCodeCoverage]
internal sealed class DbConnectionContext : IDbConnectionWithinTransaction
{
    private readonly PostgresDbSettings _postgresDbSettings;
    private readonly bool _withinTransaction;
    private bool _committed;
    private NpgsqlConnection _sqlConnection;
    private NpgsqlTransaction _sqlTransaction;

    public DbConnectionContext(PostgresDbSettings postgresDbSettings, bool withinTransaction)
    {
            Guard.Argument(postgresDbSettings).NotNull();
            _postgresDbSettings = postgresDbSettings;
            _withinTransaction = withinTransaction;
        }

    public void Commit()
    {
            if (_sqlTransaction is object)
            {
                _sqlTransaction.Commit();
                _committed = true;
            }
        }

    public NpgsqlCommand CreateCommand()
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

    private NpgsqlConnection GetDbConnection()
    {
            if (_sqlConnection is null)
            {
                _sqlConnection = new NpgsqlConnection(_postgresDbSettings.ConnectionString);
                _sqlConnection.Open();
                _sqlConnection.ChangeDatabase(_postgresDbSettings.DatabaseName);
            }
            return _sqlConnection;
        }

    private NpgsqlTransaction GetDbTransaction()
    {
            if (_sqlTransaction is null)
            {
                _sqlTransaction = GetDbConnection().BeginTransaction();
            }
            return _sqlTransaction;
        }
}