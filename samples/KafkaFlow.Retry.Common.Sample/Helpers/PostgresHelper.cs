using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Npgsql;

namespace KafkaFlow.Retry.Common.Sample.Helpers;

public static class PostgresHelper
{
    public static async Task RecreateSqlSchema(string databaseName, string connectionString)
    {
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