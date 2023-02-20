using System.Collections.Generic;
using System.Threading.Tasks;
using Dawn;
using KafkaFlow.Retry.Postgres.Model.Schema;
using Npgsql;

namespace KafkaFlow.Retry.Postgres
{
    internal class RetrySchemaCreator : IRetrySchemaCreator
    {
        private readonly IEnumerable<Script> schemaScripts;
        private readonly PostgresDbSettings postgresDbSettings;

        public RetrySchemaCreator(PostgresDbSettings postgresDbSettings, IEnumerable<Script> schemaScripts)
        {
            Guard.Argument(postgresDbSettings, nameof(postgresDbSettings)).NotNull();
            Guard.Argument(schemaScripts, nameof(schemaScripts)).NotNull();

            this.postgresDbSettings = postgresDbSettings;
            this.schemaScripts = schemaScripts;
        }

        public async Task CreateOrUpdateSchemaAsync(string databaseName)
        {
            using (NpgsqlConnection openCon = new NpgsqlConnection(this.postgresDbSettings.ConnectionString))
            {
                openCon.Open();

                foreach (var script in this.schemaScripts)
                {
                    string[] batches = script.Value.Split(new string[] { "GO\r\n", "GO\t", "GO\n" }, System.StringSplitOptions.RemoveEmptyEntries);

                    foreach (var batch in batches)
                    {
                        string replacedBatch = batch.Replace("@dbname", databaseName);

                        using (NpgsqlCommand queryCommand = new NpgsqlCommand(replacedBatch))
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