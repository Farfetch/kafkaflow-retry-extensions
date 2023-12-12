using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dawn;
using KafkaFlow.Retry.Postgres.Model;
using Npgsql;
using NpgsqlTypes;

namespace KafkaFlow.Retry.Postgres.Repositories;

internal sealed class RetryQueueItemMessageRepository : IRetryQueueItemMessageRepository
{
    public async Task AddAsync(IDbConnection dbConnection, RetryQueueItemMessageDbo retryQueueItemMessageDbo)
    {
            Guard.Argument(dbConnection, nameof(dbConnection)).NotNull();
            Guard.Argument(retryQueueItemMessageDbo, nameof(retryQueueItemMessageDbo)).NotNull();

            using (var command = dbConnection.CreateCommand())
            {
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = @"INSERT INTO item_messages
                                            (IdRetryQueueItem, ""key"", Value, TopicName, Partition, ""offset"", UtcTimeStamp)
                                        VALUES
                                            (@idRetryQueueItem, @key, @value, @topicName, @partition, @offSet, @utcTimeStamp)";

                command.Parameters.AddWithValue("idRetryQueueItem", retryQueueItemMessageDbo.IdRetryQueueItem);
                command.Parameters.AddWithValue("key", retryQueueItemMessageDbo.Key);
                command.Parameters.AddWithValue("value", retryQueueItemMessageDbo.Value);
                command.Parameters.AddWithValue("topicName", retryQueueItemMessageDbo.TopicName);
                command.Parameters.AddWithValue("partition", retryQueueItemMessageDbo.Partition);
                command.Parameters.AddWithValue("offset", retryQueueItemMessageDbo.Offset);
                command.Parameters.AddWithValue("utcTimeStamp", retryQueueItemMessageDbo.UtcTimeStamp);

                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

    public async Task<IList<RetryQueueItemMessageDbo>> GetMessagesOrderedAsync(IDbConnection dbConnection, IEnumerable<RetryQueueItemDbo> retryQueueItemsDbo)
    {
            Guard.Argument(dbConnection, nameof(dbConnection)).NotNull();
            Guard.Argument(retryQueueItemsDbo, nameof(retryQueueItemsDbo)).NotNull();

            using (var command = dbConnection.CreateCommand())
            {
                var retryQueueItemIds = retryQueueItemsDbo.Select(x => x.Id);
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = $@"SELECT * FROM f_load_item_messages(@retryQueueItemIds)";
                command.Parameters.AddWithValue("retryQueueItemIds", 
                    NpgsqlDbType.Array | NpgsqlDbType.Bigint, retryQueueItemIds.ToArray());

                return await ExecuteReaderAsync(command).ConfigureAwait(false);
            }
        }

    private async Task<IList<RetryQueueItemMessageDbo>> ExecuteReaderAsync(NpgsqlCommand command)
    {
            var messages = new List<RetryQueueItemMessageDbo>();

            using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    messages.Add(FillDbo(reader));
                }
            }

            return messages;
        }

    private RetryQueueItemMessageDbo FillDbo(NpgsqlDataReader reader)
    {
            return new RetryQueueItemMessageDbo
            {
                IdRetryQueueItem = reader.GetInt64(reader.GetOrdinal("IdRetryQueueItem")),
                Key = (byte[])reader["Key"],
                Offset = reader.GetInt64(reader.GetOrdinal("offset")),
                Partition = reader.GetInt32(reader.GetOrdinal("Partition")),
                TopicName = reader.GetString(reader.GetOrdinal("TopicName")),
                UtcTimeStamp = reader.GetDateTime(reader.GetOrdinal("UtcTimeStamp")),
                Value = (byte[])reader["Value"]
            };
        }
}