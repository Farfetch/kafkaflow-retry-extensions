using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace KafkaFlow.Retry.IntegrationTests.Core.Bootstrappers;

internal static class BootstrapperPostgresSchema
{
    private static readonly SemaphoreSlim s_semaphoreOneThreadAtTime = new(1, 1);
    private static bool s_schemaInitialized;

    internal static async Task RecreatePostgresSchemaAsync(string databaseName, string connectionString)
    {
        await s_semaphoreOneThreadAtTime.WaitAsync();
        try
        {
            if (s_schemaInitialized)
            {
                return;
            }

            await using (var openCon = new NpgsqlConnection(connectionString))
            {
                openCon.Open();
                openCon.ChangeDatabase(databaseName);

                var scripts = GetScriptsForSchemaCreation();

                foreach (var script in scripts)
                {
                    await using (var queryCommand = new NpgsqlCommand(script))
                    {
                        queryCommand.Connection = openCon;

                        await queryCommand.ExecuteNonQueryAsync();
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
        var postgresAssembly = Assembly.LoadFrom("KafkaFlow.Retry.Postgres.dll");
        return postgresAssembly
            .GetManifestResourceNames()
            .OrderBy(x => x)
            .Select(script =>
            {
                using (var s = postgresAssembly.GetManifestResourceStream(script))
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