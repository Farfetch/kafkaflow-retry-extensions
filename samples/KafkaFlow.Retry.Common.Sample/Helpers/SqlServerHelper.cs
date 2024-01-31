using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace KafkaFlow.Retry.Common.Sample.Helpers;

public static class SqlServerHelper
{
    public static async Task RecreateSqlSchema(string databaseName, string connectionString)
    {
        using (var openCon = new SqlConnection(connectionString))
        {
            openCon.Open();

            foreach (var script in GetScriptsForSchemaCreation())
            {
                var batches = script.Split(new[] { "GO\r\n", "GO\t", "GO\n" }, StringSplitOptions.RemoveEmptyEntries);

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