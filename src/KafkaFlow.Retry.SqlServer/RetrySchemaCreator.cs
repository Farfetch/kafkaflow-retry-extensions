using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using Dawn;
using KafkaFlow.Retry.SqlServer.Model.Schema;

namespace KafkaFlow.Retry.SqlServer;

internal class RetrySchemaCreator : IRetrySchemaCreator
{
    private readonly IEnumerable<Script> _schemaScripts;
    private readonly SqlServerDbSettings _sqlServerDbSettings;

    public RetrySchemaCreator(SqlServerDbSettings sqlServerDbSettings, IEnumerable<Script> schemaScripts)
    {
            Guard.Argument(sqlServerDbSettings, nameof(sqlServerDbSettings)).NotNull();
            Guard.Argument(schemaScripts, nameof(schemaScripts)).NotNull();

            _sqlServerDbSettings = sqlServerDbSettings;
            _schemaScripts = schemaScripts;
        }

    public async Task CreateOrUpdateSchemaAsync(string databaseName)
    {
            using (SqlConnection openCon = new SqlConnection(_sqlServerDbSettings.ConnectionString))
            {
                openCon.Open();

                foreach (var script in _schemaScripts)
                {
                    string[] batches = script.Value.Split(new string[] { "GO\r\n", "GO\t", "GO\n" }, System.StringSplitOptions.RemoveEmptyEntries);

                    foreach (var batch in batches)
                    {
                        string replacedBatch = batch.Replace("@dbname", databaseName);

                        using (SqlCommand queryCommand = new SqlCommand(replacedBatch))
                        {
                            queryCommand.Connection = openCon;

                            await queryCommand.ExecuteNonQueryAsync().ConfigureAwait(false);
                        }
                    }
                }
            }
        }
}