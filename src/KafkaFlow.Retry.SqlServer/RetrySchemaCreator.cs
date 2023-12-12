using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dawn;
using KafkaFlow.Retry.SqlServer.Model.Schema;
using Microsoft.Data.SqlClient;

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
        using (var openCon = new SqlConnection(_sqlServerDbSettings.ConnectionString))
        {
            openCon.Open();

            foreach (var script in _schemaScripts)
            {
                var batches = script.Value.Split(new[] { "GO\r\n", "GO\t", "GO\n" },
                    StringSplitOptions.RemoveEmptyEntries);

                foreach (var batch in batches)
                {
                    var replacedBatch = batch.Replace("@dbname", databaseName);

                    using (var queryCommand = new SqlCommand(replacedBatch))
                    {
                        queryCommand.Connection = openCon;

                        await queryCommand.ExecuteNonQueryAsync().ConfigureAwait(false);
                    }
                }
            }
        }
    }
}