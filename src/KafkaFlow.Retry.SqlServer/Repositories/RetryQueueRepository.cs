using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using KafkaFlow.Retry.Durable.Repository.Actions.Read;
using KafkaFlow.Retry.Durable.Repository.Model;
using KafkaFlow.Retry.SqlServer.Model;

namespace KafkaFlow.Retry.SqlServer.Repositories;
internal sealed class RetryQueueRepository : IRetryQueueRepository
{
    public async Task<long> AddAsync(IDbConnection dbConnection, RetryQueueDbo retryQueueDbo)
    {
            using (var command = dbConnection.CreateCommand())
            {
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = $@"INSERT INTO {dbConnection.Schema}.[RetryQueues]
                                            (IdDomain, IdStatus, SearchGroupKey, QueueGroupKey, CreationDate, LastExecution)
                                        VALUES
                                            (@idDomain, @idStatus, @searchGroupKey, @queueGroupKey, @creationDate, @lastExecution);

                                        SELECT SCOPE_IDENTITY()";

                command.Parameters.AddWithValue("idDomain", retryQueueDbo.IdDomain);
                command.Parameters.AddWithValue("idStatus", retryQueueDbo.Status);
                command.Parameters.AddWithValue("searchGroupKey", retryQueueDbo.SearchGroupKey);
                command.Parameters.AddWithValue("queueGroupKey", retryQueueDbo.QueueGroupKey);
                command.Parameters.AddWithValue("creationDate", retryQueueDbo.CreationDate);
                command.Parameters.AddWithValue("lastExecution", retryQueueDbo.LastExecution);

                return Convert.ToInt64(await command.ExecuteScalarAsync().ConfigureAwait(false));
            }
        }

    public async Task<int> DeleteQueuesAsync(IDbConnection dbConnection, string searchGroupKey, RetryQueueStatus retryQueueStatus, DateTime maxLastExecutionDateToBeKept, int maxRowsToDelete)
    {
            using (var command = dbConnection.CreateCommand())
            {
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = $@"DELETE FROM [{dbConnection.Schema}].[RetryQueues] WHERE Id IN
                                        (
                                            SELECT Id FROM [{dbConnection.Schema}].[RetryQueues] rq
                                                WHERE rq.SearchGroupKey = @SearchGroupKey
                                                AND rq.LastExecution < @MaxLastExecutionDateToBeKept
                                                AND rq.IdStatus = @IdStatus
                                                ORDER BY 1
                                                OFFSET 0 ROWS
                                                FETCH NEXT @MaxRowsToDelete ROWS ONLY
                                        )";

                command.Parameters.AddWithValue("SearchGroupKey", searchGroupKey);
                command.Parameters.AddWithValue("MaxLastExecutionDateToBeKept", maxLastExecutionDateToBeKept);
                command.Parameters.AddWithValue("IdStatus", (byte)retryQueueStatus);
                command.Parameters.AddWithValue("MaxRowsToDelete", maxRowsToDelete);

                return await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

    public async Task<bool> ExistsActiveAsync(IDbConnection dbConnection, string queueGroupKey)
    {
            using (var command = dbConnection.CreateCommand())
            {
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = $@"SELECT COUNT(1)
                                        FROM [{dbConnection.Schema}].[RetryQueues]
                                        WHERE QueueGroupKey = @QueueGroupKey AND IdStatus <> @IdStatus";

                command.Parameters.AddWithValue("QueueGroupKey", queueGroupKey);
                command.Parameters.AddWithValue("IdStatus", RetryQueueStatus.Done);

                return Convert.ToInt32(await command.ExecuteScalarAsync().ConfigureAwait(false)) > 0;
            }
        }

    public async Task<RetryQueueDbo> GetQueueAsync(IDbConnection dbConnection, string queueGroupKey)
    {
            using (var command = dbConnection.CreateCommand())
            {
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = $@"SELECT Id, IdDomain, IdStatus, SearchGroupKey, QueueGroupKey, CreationDate, LastExecution
                                        FROM [{dbConnection.Schema}].[RetryQueues]
                                        WHERE QueueGroupKey = @QueueGroupKey
                                        ORDER BY Id";

                command.Parameters.AddWithValue("QueueGroupKey", queueGroupKey);

                return await ExecuteSingleLineReaderAsync(command).ConfigureAwait(false);
            }
        }

    public async Task<IList<RetryQueueDbo>> GetTopSortedQueuesOrderedAsync(IDbConnection dbConnection, RetryQueueStatus retryQueueStatus, GetQueuesSortOption sortOption, string searchGroupKey, int top)
    {
            using (var command = dbConnection.CreateCommand())
            {
                command.CommandType = System.Data.CommandType.Text;

                var innerQuery = $@" SELECT TOP({top}) Id, IdDomain, IdStatus, SearchGroupKey, QueueGroupKey, CreationDate, LastExecution
                                        FROM [{dbConnection.Schema}].[RetryQueues]
                                        WHERE IdStatus = @IdStatus";

                if (searchGroupKey is object)
                {
                    innerQuery = string.Concat(innerQuery, $" AND SearchGroupKey = '{searchGroupKey}' ");
                }

                innerQuery = string.Concat(innerQuery, GetOrderByCommandString(sortOption));

                var orderedByIdQuery = String.Concat("; WITH SortedItems AS ( ", innerQuery, " ) SELECT * FROM SortedItems ORDER BY Id ");

                command.CommandText = orderedByIdQuery;

                command.Parameters.AddWithValue("IdStatus", (byte)retryQueueStatus);

                return await ExecuteReaderAsync(command).ConfigureAwait(false);
            }
        }

    public async Task<int> UpdateAsync(IDbConnection dbConnection, Guid idDomain, RetryQueueStatus retryQueueStatus, DateTime lastExecution)
    {
            using (var command = dbConnection.CreateCommand())
            {
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = $@"UPDATE [{dbConnection.Schema}].[RetryQueues]
                                      SET IdStatus = @IdStatus,
                                          LastExecution = @LastExecution
                                      WHERE IdDomain = @IdDomain";

                command.Parameters.AddWithValue("IdDomain", idDomain);
                command.Parameters.AddWithValue("IdStatus", (byte)retryQueueStatus);
                command.Parameters.AddWithValue("LastExecution", lastExecution);

                return await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

    public async Task<int> UpdateLastExecutionAsync(IDbConnection dbConnection, Guid idDomain, DateTime lastExecution)
    {
            using (var command = dbConnection.CreateCommand())
            {
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = $@"UPDATE [{dbConnection.Schema}].[RetryQueues]
                                      SET LastExecution = @LastExecution
                                      WHERE IdDomain = @IdDomain";

                command.Parameters.AddWithValue("IdDomain", idDomain);
                command.Parameters.AddWithValue("LastExecution", lastExecution);

                return await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

    public async Task<int> UpdateStatusAsync(IDbConnection dbConnection, Guid idDomain, RetryQueueStatus retryQueueStatus)
    {
            using (var command = dbConnection.CreateCommand())
            {
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = $@"UPDATE [{dbConnection.Schema}].[RetryQueues]
                                      SET IdStatus = @IdStatus
                                      WHERE IdDomain = @IdDomain";

                command.Parameters.AddWithValue("IdDomain", idDomain);
                command.Parameters.AddWithValue("IdStatus", (byte)retryQueueStatus);

                return await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

    private async Task<IList<RetryQueueDbo>> ExecuteReaderAsync(SqlCommand command)
    {
            var queues = new List<RetryQueueDbo>();

            using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    queues.Add(FillDbo(reader));
                }
            }

            return queues;
        }

    private async Task<RetryQueueDbo> ExecuteSingleLineReaderAsync(SqlCommand command)
    {
            using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
            {
                if (await reader.ReadAsync().ConfigureAwait(false))
                {
                    return FillDbo(reader);
                }
            }

            return null;
        }

    private RetryQueueDbo FillDbo(SqlDataReader reader)
    {
            return new RetryQueueDbo
            {
                Id = reader.GetInt64(reader.GetOrdinal("Id")),
                IdDomain = reader.GetGuid(reader.GetOrdinal("IdDomain")),
                CreationDate = reader.GetDateTime(reader.GetOrdinal("CreationDate")),
                LastExecution = reader.GetDateTime(reader.GetOrdinal("LastExecution")),
                QueueGroupKey = reader.GetString(reader.GetOrdinal("QueueGroupKey")),
                SearchGroupKey = reader.GetString(reader.GetOrdinal("SearchGroupKey")),
                Status = (RetryQueueStatus)reader.GetByte(reader.GetOrdinal("IdStatus"))
            };
        }

    private string GetOrderByCommandString(GetQueuesSortOption sortOption)
    {
            switch (sortOption)
            {
                case GetQueuesSortOption.ByCreationDateDescending:
                    return " ORDER BY CreationDate DESC";

                case GetQueuesSortOption.ByLastExecutionAscending:
                default:
                    return " ORDER BY LastExecution ASC";
            }
        }
}