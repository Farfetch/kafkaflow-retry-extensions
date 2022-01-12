namespace KafkaFlow.Retry.IntegrationTests.Core.Bootstrappers
{
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    internal static class BootstrapperSqlServerSchema
    {
        private static readonly SemaphoreSlim semaphoreOneThreadAtTime = new SemaphoreSlim(1, 1);
        private static bool schemaInitialized;

        internal static async Task RecreateSqlSchemaAsync(string databaseName, string connectionString)
        {
            await semaphoreOneThreadAtTime.WaitAsync();
            try
            {
                System.Console.WriteLine($"[User LOG] SqlServerSchema init. SchemaInitialized: {schemaInitialized}");
                if (schemaInitialized)
                {
                    return;
                }

                using (SqlConnection openCon = new SqlConnection(connectionString))
                {
                    openCon.Open();
                    var scripts = GetScriptsForSchemaCreation();
                    System.Console.WriteLine($"[User LOG] SqlServerSchema executing {scripts.Count()} scripts.");
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
                    System.Console.WriteLine($"[User LOG] SqlServerSchema {scripts.Count()} scripts executed.");
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
}