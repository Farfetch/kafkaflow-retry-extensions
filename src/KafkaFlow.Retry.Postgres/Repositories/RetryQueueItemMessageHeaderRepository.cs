using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dawn;
using KafkaFlow.Retry.Postgres.Model;
using Npgsql;

namespace KafkaFlow.Retry.Postgres.Repositories;

internal sealed class RetryQueueItemMessageHeaderRepository : IRetryQueueItemMessageHeaderRepository
{
    public async Task AddAsync(IDbConnection dbConnection,
        IEnumerable<RetryQueueItemMessageHeaderDbo> retryQueueHeadersDbo)
    {
        Guard.Argument(dbConnection, nameof(dbConnection)).NotNull();
        Guard.Argument(retryQueueHeadersDbo, nameof(retryQueueHeadersDbo)).NotNull();

        foreach (var header in retryQueueHeadersDbo)
        {
            await AddAsync(dbConnection, header).ConfigureAwait(false);
        }
    }

    public async Task<IList<RetryQueueItemMessageHeaderDbo>> GetOrderedAsync(IDbConnection dbConnection,
        IEnumerable<RetryQueueItemMessageDbo> retryQueueItemMessagesDbo)
    {
        Guard.Argument(dbConnection, nameof(dbConnection)).NotNull();
        Guard.Argument(retryQueueItemMessagesDbo, nameof(retryQueueItemMessagesDbo)).NotNull();

        using (var command = dbConnection.CreateCommand())
        {
            command.CommandType = CommandType.Text;
            command.CommandText = $@"SELECT *
                                         FROM retry_item_message_headers h
                                         INNER JOIN retry_queue_items rqi ON rqi.Id = h.IdItemMessage
                                         WHERE h.IdItemMessage IN ({string.Join(",", retryQueueItemMessagesDbo.Select(x => $"'{x.IdRetryQueueItem}'"))})
                                         ORDER BY rqi.IdRetryQueue, h.IdItemMessage";

            return await ExecuteReaderAsync(command).ConfigureAwait(false);
        }
    }

    private async Task AddAsync(IDbConnection dbConnection, RetryQueueItemMessageHeaderDbo retryQueueHeaderDbo)
    {
        Guard.Argument(dbConnection, nameof(dbConnection)).NotNull();
        Guard.Argument(retryQueueHeaderDbo, nameof(retryQueueHeaderDbo)).NotNull();

        using (var command = dbConnection.CreateCommand())
        {
            command.CommandType = CommandType.Text;
            command.CommandText = @"INSERT INTO retry_item_message_headers
                                            (IdItemMessage, ""key"", Value)
                                        VALUES
                                            (@IdItemMessage, @Key, @Value)";

            command.Parameters.AddWithValue("IdItemMessage", retryQueueHeaderDbo.RetryQueueItemMessageId);
            command.Parameters.AddWithValue("Key", retryQueueHeaderDbo.Key);
            command.Parameters.AddWithValue("Value", retryQueueHeaderDbo.Value);

            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
        }
    }

    private async Task<IList<RetryQueueItemMessageHeaderDbo>> ExecuteReaderAsync(NpgsqlCommand command)
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

    private RetryQueueItemMessageHeaderDbo FillDbo(NpgsqlDataReader reader, int idColumn, int keyColumn,
        int retryQueueItemMessageColumn, int valueColumn)
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