namespace KafkaFlow.Retry.SqlServer
{
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Threading.Tasks;
    using Dawn;
    using KafkaFlow.Retry.SqlServer.Model.Schema;

    internal class RetrySchemaCreator : IRetrySchemaCreator
    {
        private readonly IEnumerable<Script> schemaScripts;
        private readonly SqlServerDbSettings sqlServerDbSettings;

        public RetrySchemaCreator(SqlServerDbSettings sqlServerDbSettings, IEnumerable<Script> schemaScripts)
        {
            Guard.Argument(sqlServerDbSettings, nameof(sqlServerDbSettings)).NotNull();
            Guard.Argument(schemaScripts, nameof(schemaScripts)).NotNull();

            this.sqlServerDbSettings = sqlServerDbSettings;
            this.schemaScripts = schemaScripts;
        }

        public async Task CreateOrUpdateSchemaAsync(string databaseName)
        {
            using (SqlConnection openCon = new SqlConnection(this.sqlServerDbSettings.ConnectionString))
            {
                openCon.Open();

                foreach (var script in this.schemaScripts)
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
}