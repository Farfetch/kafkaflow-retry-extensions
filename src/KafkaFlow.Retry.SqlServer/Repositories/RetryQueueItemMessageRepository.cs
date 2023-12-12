using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using Dawn;
using KafkaFlow.Retry.SqlServer.Model;

namespace KafkaFlow.Retry.SqlServer.Repositories;

internal sealed class RetryQueueItemMessageRepository : IRetryQueueItemMessageRepository
{
    public async Task AddAsync(IDbConnection dbConnection, RetryQueueItemMessageDbo retryQueueItemMessageDbo)
    {
        Guard.Argument(dbConnection, nameof(dbConnection)).NotNull();
        Guard.Argument(retryQueueItemMessageDbo, nameof(retryQueueItemMessageDbo)).NotNull();

        using (var command = dbConnection.CreateCommand())
        {
            command.CommandType = System.Data.CommandType.Text;
            command.CommandText = $@"INSERT INTO [{dbConnection.Schema}].[ItemMessages]
                                            (IdRetryQueueItem, [Key], Value, TopicName, Partition, Offset, UtcTimeStamp)
                                        VALUES
                                            (@idRetryQueueItem, @key, @value, @topicName, @partition, @offSet, @utcTimeStamp)";

            command.Parameters.AddWithValue("idRetryQueueItem", retryQueueItemMessageDbo.IdRetryQueueItem);
            command.Parameters.AddWithValue("key", retryQueueItemMessageDbo.Key);
            command.Parameters.AddWithValue("value", retryQueueItemMessageDbo.Value);
            command.Parameters.AddWithValue("topicName", retryQueueItemMessageDbo.TopicName);
            command.Parameters.AddWithValue("partition", retryQueueItemMessageDbo.Partition);
            command.Parameters.AddWithValue("offSet", retryQueueItemMessageDbo.Offset);
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
            var entriesToLoad = new System.Data.DataTable("TY_RetryQueueItemsIds");
            entriesToLoad.Columns.Add("Id", typeof(int));
            foreach (var retryQueueItemDbo in retryQueueItemsDbo)
            {
                var dr = entriesToLoad.NewRow();
                dr["Id"] = retryQueueItemDbo.Id;
                entriesToLoad.Rows.Add(dr);
            }
            var parameter = new SqlParameter("@RetryQueueItemsIds", entriesToLoad);
            parameter.Direction = System.Data.ParameterDirection.Input;
            parameter.TypeName = $"{dbConnection.Schema}.TY_RetryQueueItemsIds";

            command.Parameters.Add(parameter);
            command.CommandType = System.Data.CommandType.Text;
            command.CommandText = $@"EXEC {dbConnection.Schema}.P_LoadItemMessages @RetryQueueItemsIds";

            return await this.ExecuteReaderAsync(command).ConfigureAwait(false);
        }
    }

    private async Task<IList<RetryQueueItemMessageDbo>> ExecuteReaderAsync(SqlCommand command)
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

    private RetryQueueItemMessageDbo FillDbo(SqlDataReader reader)
    {
        return new RetryQueueItemMessageDbo
        {
            IdRetryQueueItem = reader.GetInt64(reader.GetOrdinal("IdRetryQueueItem")),
            Key = (byte[])reader["Key"],
            Offset = reader.GetInt64(reader.GetOrdinal("Offset")),
            Partition = reader.GetInt32(reader.GetOrdinal("Partition")),
            TopicName = reader.GetString(reader.GetOrdinal("TopicName")),
            UtcTimeStamp = reader.GetDateTime(reader.GetOrdinal("UtcTimeStamp")),
            Value = (byte[])reader["Value"]
        };
    }
}