using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KafkaFlow.Retry.Durable.Repository.Actions.Read;
using KafkaFlow.Retry.Durable.Repository.Model;
using KafkaFlow.Retry.Postgres.Model;
using Npgsql;

namespace KafkaFlow.Retry.Postgres.Repositories
{
    internal sealed class RetryQueueRepository : IRetryQueueRepository
    {
        public async Task<long> AddAsync(IDbConnection dbConnection, RetryQueueDbo retryQueueDbo)
        {
            using (var command = dbConnection.CreateCommand())
            {
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = @"INSERT INTO retry_queues
                                            (IdDomain, IdStatus, SearchGroupKey, QueueGroupKey, CreationDate, LastExecution)
                                        VALUES
                                            (@idDomain, @idStatus, @searchGroupKey, @queueGroupKey, @creationDate, @lastExecution)
                                        RETURNING id";

                command.Parameters.AddWithValue("idDomain", retryQueueDbo.IdDomain);
                command.Parameters.AddWithValue("idStatus", (byte) retryQueueDbo.Status);
                command.Parameters.AddWithValue("searchGroupKey", retryQueueDbo.SearchGroupKey);
                command.Parameters.AddWithValue("queueGroupKey", retryQueueDbo.QueueGroupKey);
                command.Parameters.AddWithValue("creationDate", retryQueueDbo.CreationDate);
                command.Parameters.AddWithValue("lastExecution", retryQueueDbo.LastExecution);

                return Convert.ToInt64(await command.ExecuteScalarAsync().ConfigureAwait(false));
            }
        }

        public async Task<bool> ExistsActiveAsync(IDbConnection dbConnection, string queueGroupKey)
        {
            using (var command = dbConnection.CreateCommand())
            {
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = @"SELECT COUNT(1)
                                        FROM retry_queues
                                        WHERE QueueGroupKey = @QueueGroupKey AND IdStatus <> @IdStatus";

                command.Parameters.AddWithValue("QueueGroupKey", queueGroupKey);
                command.Parameters.AddWithValue("IdStatus", (byte) RetryQueueStatus.Done);

                return Convert.ToInt32(await command.ExecuteScalarAsync().ConfigureAwait(false)) > 0;
            }
        }

        public async Task<RetryQueueDbo> GetQueueAsync(IDbConnection dbConnection, string queueGroupKey)
        {
            using (var command = dbConnection.CreateCommand())
            {
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = @"SELECT Id, IdDomain, IdStatus, SearchGroupKey, QueueGroupKey, CreationDate, LastExecution
                                        FROM retry_queues
                                        WHERE QueueGroupKey = @QueueGroupKey
                                        ORDER BY Id";

                command.Parameters.AddWithValue("QueueGroupKey", queueGroupKey);

                return await this.ExecuteSingleLineReaderAsync(command).ConfigureAwait(false);
            }
        }

        public async Task<IList<RetryQueueDbo>> GetTopSortedQueuesOrderedAsync(IDbConnection dbConnection, RetryQueueStatus retryQueueStatus, GetQueuesSortOption sortOption, string searchGroupKey, int top)
        {
            using (var command = dbConnection.CreateCommand())
            {
                command.CommandType = System.Data.CommandType.Text;

                var innerQuery = $@" SELECT Id, IdDomain, IdStatus, SearchGroupKey, QueueGroupKey, CreationDate, LastExecution
                                        FROM retry_queues
                                        WHERE IdStatus = @IdStatus";

                if (searchGroupKey is object)
                {
                    innerQuery = string.Concat(innerQuery, $" AND SearchGroupKey = '{searchGroupKey}' ");
                }

                innerQuery = string.Concat(innerQuery, this.GetOrderByCommandString(sortOption));
                innerQuery = string.Concat(innerQuery, $" LIMIT {top}");

                var orderedByIdQuery = String.Concat("; WITH SortedItems AS ( ", innerQuery, " ) SELECT * FROM SortedItems ORDER BY Id ");

                command.CommandText = orderedByIdQuery;

                command.Parameters.AddWithValue("IdStatus", (byte)retryQueueStatus);

                return await this.ExecuteReaderAsync(command).ConfigureAwait(false);
            }
        }

        public async Task<int> UpdateAsync(IDbConnection dbConnection, Guid idDomain, RetryQueueStatus retryQueueStatus, DateTime lastExecution)
        {
            using (var command = dbConnection.CreateCommand())
            {
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = @"UPDATE retry_queues
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
                command.CommandText = @"UPDATE retry_queues
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
                command.CommandText = @"UPDATE retry_queues
                                      SET IdStatus = @IdStatus
                                      WHERE IdDomain = @IdDomain";

                command.Parameters.AddWithValue("IdDomain", idDomain);
                command.Parameters.AddWithValue("IdStatus", (byte)retryQueueStatus);

                return await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        private async Task<IList<RetryQueueDbo>> ExecuteReaderAsync(NpgsqlCommand command)
        {
            var queues = new List<RetryQueueDbo>();

            using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    queues.Add(this.FillDbo(reader));
                }
            }

            return queues;
        }

        private async Task<RetryQueueDbo> ExecuteSingleLineReaderAsync(NpgsqlCommand command)
        {
            using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
            {
                if (await reader.ReadAsync().ConfigureAwait(false))
                {
                    return this.FillDbo(reader);
                }
            }

            return null;
        }

        private RetryQueueDbo FillDbo(NpgsqlDataReader reader)
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
                case GetQueuesSortOption.ByCreationDate_Descending:
                    return " ORDER BY CreationDate DESC";

                case GetQueuesSortOption.ByLastExecution_Ascending:
                default:
                    return " ORDER BY LastExecution ASC";
            }
        }
    }
}
