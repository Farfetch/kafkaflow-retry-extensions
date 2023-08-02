namespace KafkaFlow.Retry.SqlServer.Repositories
{
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Threading.Tasks;
    using Dawn;
    using KafkaFlow.Retry.SqlServer.Model;

    internal sealed class RetryQueueItemMessageHeaderRepository : IRetryQueueItemMessageHeaderRepository
    {
        public async Task AddAsync(IDbConnection dbConnection, IEnumerable<RetryQueueItemMessageHeaderDbo> retryQueueHeadersDbo, string schema)
        {
            Guard.Argument(dbConnection, nameof(dbConnection)).NotNull();
            Guard.Argument(retryQueueHeadersDbo, nameof(retryQueueHeadersDbo)).NotNull();

            foreach (var header in retryQueueHeadersDbo)
            {
                await this.AddAsync(dbConnection, header, schema).ConfigureAwait(false);
            }
        }

        public async Task<IList<RetryQueueItemMessageHeaderDbo>> GetOrderedAsync(IDbConnection dbConnection, IEnumerable<RetryQueueItemMessageDbo> retryQueueItemMessagesDbo, string schema)
        {
            Guard.Argument(dbConnection, nameof(dbConnection)).NotNull();
            Guard.Argument(retryQueueItemMessagesDbo, nameof(retryQueueItemMessagesDbo)).NotNull();

            using (var command = dbConnection.CreateCommand())
            {
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = $@"SELECT *
                                         FROM [{schema}].[RetryItemMessageHeaders] h
                                         INNER JOIN [{schema}].[RetryQueueItems] rqi ON rqi.Id = h.IdItemMessage
                                         WHERE h.IdItemMessage IN ({string.Join(",", retryQueueItemMessagesDbo.Select(x => $"'{x.IdRetryQueueItem}'"))})
                                         ORDER BY rqi.IdRetryQueue, h.IdItemMessage";

                return await this.ExecuteReaderAsync(command).ConfigureAwait(false);
            }
        }

        private async Task AddAsync(IDbConnection dbConnection, RetryQueueItemMessageHeaderDbo retryQueueHeaderDbo, string schema)
        {
            Guard.Argument(dbConnection, nameof(dbConnection)).NotNull();
            Guard.Argument(retryQueueHeaderDbo, nameof(retryQueueHeaderDbo)).NotNull();

            using (var command = dbConnection.CreateCommand())
            {
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = $@"INSERT INTO [{schema}].[RetryItemMessageHeaders]
                                            (IdItemMessage, [Key], Value)
                                        VALUES
                                            (@IdItemMessage, @Key, @Value)";

                command.Parameters.AddWithValue("IdItemMessage", retryQueueHeaderDbo.RetryQueueItemMessageId);
                command.Parameters.AddWithValue("Key", retryQueueHeaderDbo.Key);
                command.Parameters.AddWithValue("Value", retryQueueHeaderDbo.Value);

                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        private async Task<IList<RetryQueueItemMessageHeaderDbo>> ExecuteReaderAsync(SqlCommand command)
        {
            var headers = new List<RetryQueueItemMessageHeaderDbo>();

            using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
            {
                var idColumn = reader.GetOrdinal("Id");
                var keyColumn = reader.GetOrdinal("Key");
                var retryQueueItemMessageColumn = reader.GetOrdinal("IdItemMessage");
                var valueColumn = reader.GetOrdinal("Value");

                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    headers.Add(FillDbo(reader, idColumn, keyColumn, retryQueueItemMessageColumn, valueColumn));
                }
            }

            return headers;
        }

        private RetryQueueItemMessageHeaderDbo FillDbo(SqlDataReader reader, int idColumn, int keyColumn, int retryQueueItemMessageColumn, int valueColumn)
        {
            return new RetryQueueItemMessageHeaderDbo
            {
                Id = reader.GetInt64(idColumn),
                Key = reader.GetString(keyColumn),
                Value = (byte[])reader.GetValue(valueColumn),
                RetryQueueItemMessageId = reader.GetInt64(retryQueueItemMessageColumn)
            };
        }
    }
}
