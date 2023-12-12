using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace KafkaFlow.Retry.IntegrationTests.Core.Bootstrappers;

internal static class BootstrapperSqlServerSchema
{
    private static readonly SemaphoreSlim s_semaphoreOneThreadAtTime = new SemaphoreSlim(1, 1);
    private static bool s_schemaInitialized;

    internal static async Task RecreateSqlSchemaAsync(string databaseName, string connectionString)
    {
            await s_semaphoreOneThreadAtTime.WaitAsync().ConfigureAwait(false);
            try
            {
                if (s_schemaInitialized)
                {
                    return;
                }

                using (SqlConnection openCon = new SqlConnection(connectionString))
                {
                    openCon.Open();

                    var scripts = GetScriptsForSchemaCreation();

                    foreach (var script in scripts)
                    {
                        string[] batches = script.Split(new[] { "GO\r\n", "GO\t", "GO\n" }, System.StringSplitOptions.RemoveEmptyEntries);

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

                s_schemaInitialized = true;
            }
            finally
            {
                s_semaphoreOneThreadAtTime.Release();
            }
        }

    private static IEnumerable<string> GetScriptsForSchemaCreation()
    {
            Assembly sqlServerAssembly = Assembly.LoadFrom("KafkaFlow.Retry.SqlServer.dll");
            return sqlServerAssembly
                .GetManifestResourceNames()
                .OrderBy(x => x)
                .Select(script =>
                {
                    using (Stream s = sqlServerAssembly.GetManifestResourceStream(script))
                    {
                        using (StreamReader sr = new StreamReader(s))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                })
                .ToList();
        }
}