using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace KafkaFlow.Retry.IntegrationTests.Core.Bootstrappers;

internal static class BootstrapperSqlServerSchema
{
    private static readonly SemaphoreSlim s_semaphoreOneThreadAtTime = new(1, 1);
    private static bool s_schemaInitialized;

    internal static async Task RecreateSqlSchemaAsync(string databaseName, string connectionString)
    {
        await s_semaphoreOneThreadAtTime.WaitAsync();
        try
        {
            if (s_schemaInitialized)
            {
                return;
            }

            using (var openCon = new SqlConnection(connectionString))
            {
                openCon.Open();

                var scripts = GetScriptsForSchemaCreation();

                foreach (var script in scripts)
                {
                    var batches = script.Split(new[] { "GO\r\n", "GO\t", "GO\n" },
                        StringSplitOptions.RemoveEmptyEntries);

                    foreach (var batch in batches)
                    {
                        var replacedBatch = batch.Replace("@dbname", databaseName);

                        using (var queryCommand = new SqlCommand(replacedBatch))
                        {
                            queryCommand.Connection = openCon;

                            await queryCommand.ExecuteNonQueryAsync();
                        }
                    }
                }
            }

            s_schemaInitialized = true;
        }
        finally
        {
            s_semaphoreOneThreadAtTime.Release();
        }
    }

    private static IEnumerable<string> GetScriptsForSchemaCreation()
    {
        var sqlServerAssembly = Assembly.LoadFrom("KafkaFlow.Retry.SqlServer.dll");
        return sqlServerAssembly
            .GetManifestResourceNames()
            .OrderBy(x => x)
            .Select(script =>
            {
                using (var s = sqlServerAssembly.GetManifestResourceStream(script))
                {
                    using (var sr = new StreamReader(s))
                    {
                        return sr.ReadToEnd();
                    }
                }
            })
            .ToList();
    }
}