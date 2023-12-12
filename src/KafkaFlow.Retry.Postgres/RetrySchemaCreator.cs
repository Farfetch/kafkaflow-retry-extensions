using System.Collections.Generic;
using System.Threading.Tasks;
using Dawn;
using KafkaFlow.Retry.Postgres.Model.Schema;
using Npgsql;

namespace KafkaFlow.Retry.Postgres;

internal class RetrySchemaCreator : IRetrySchemaCreator
{
    private readonly IEnumerable<Script> _schemaScripts;
    private readonly PostgresDbSettings _postgresDbSettings;

    public RetrySchemaCreator(PostgresDbSettings postgresDbSettings, IEnumerable<Script> schemaScripts)
    {
            Guard.Argument(postgresDbSettings, nameof(postgresDbSettings)).NotNull();
            Guard.Argument(schemaScripts, nameof(schemaScripts)).NotNull();

            _postgresDbSettings = postgresDbSettings;
            _schemaScripts = schemaScripts;
        }

    public async Task CreateOrUpdateSchemaAsync(string databaseName)
    {
            using (var openCon = new NpgsqlConnection(_postgresDbSettings.ConnectionString))
            {
                openCon.Open();

                foreach (var script in _schemaScripts)
                {
                    var batch = script.Value;

                    using (var queryCommand = new NpgsqlCommand(batch))
                    {
                        queryCommand.Connection = openCon;

                        await queryCommand.ExecuteNonQueryAsync().ConfigureAwait(false);
                    }
                }
            }
        }
}