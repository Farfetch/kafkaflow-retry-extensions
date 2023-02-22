namespace KafkaFlow.Retry.IntegrationTests.Core.Bootstrappers
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Npgsql;

    internal static class BootstrapperPostgresSchema
    {
        private static readonly SemaphoreSlim semaphoreOneThreadAtTime = new SemaphoreSlim(1, 1);
        private static bool schemaInitialized;

        internal static async Task RecreatePostgresSchemaAsync(string databaseName, string connectionString)
        {
            await semaphoreOneThreadAtTime.WaitAsync().ConfigureAwait(false);
            try
            {
                if (schemaInitialized)
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

                            await queryCommand.ExecuteNonQueryAsync().ConfigureAwait(false);
                        }
                    }
                }

                schemaInitialized = true;
            }
            finally
            {
                semaphoreOneThreadAtTime.Release();
            }
        }

        private static IEnumerable<string> GetScriptsForSchemaCreation()
        {
            Assembly postgresAssembly = Assembly.LoadFrom("KafkaFlow.Retry.Postgres.dll");
            return postgresAssembly
                .GetManifestResourceNames()
                .OrderBy(x => x)
                .Select(script =>
                {
                    using (Stream s = postgresAssembly.GetManifestResourceStream(script))
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
}