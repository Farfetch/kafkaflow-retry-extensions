using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dawn;
using KafkaFlow.Retry.Durable.Common;
using KafkaFlow.Retry.Durable.Repository.Actions.Read;
using KafkaFlow.Retry.Durable.Repository.Model;
using KafkaFlow.Retry.Postgres.Model;
using Npgsql;

namespace KafkaFlow.Retry.Postgres.Repositories
{
    internal sealed class RetryQueueItemRepository : IRetryQueueItemRepository
    {
        public async Task<long> AddAsync(IDbConnection dbConnection, RetryQueueItemDbo retryQueueItemDbo)
        {
            Guard.Argument(dbConnection).NotNull();
            Guard.Argument(retryQueueItemDbo).NotNull();

            using (var command = dbConnection.CreateCommand())
            {
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = @"INSERT INTO retry_queue_items
                                            (IdDomain, IdRetryQueue, IdDomainRetryQueue, IdItemStatus, IdSeverityLevel, AttemptsCount, Sort, CreationDate, LastExecution, ModifiedStatusDate, Description)
                                      VALUES
                                            (@idDomain, @idRetryQueue, @idDomainRetryQueue, @idItemStatus, @idSeverityLevel, @attemptsCount,
                                             (SELECT COUNT(1) FROM retry_queue_items WHERE IdDomainRetryQueue = @idDomainRetryQueue),
                                             @creationDate, @lastExecution, @modifiedStatusDate, @description)
                                      RETURNING id";

                command.Parameters.AddWithValue("idDomain", retryQueueItemDbo.IdDomain);
                command.Parameters.AddWithValue("idRetryQueue", retryQueueItemDbo.RetryQueueId);
                command.Parameters.AddWithValue("idDomainRetryQueue", retryQueueItemDbo.DomainRetryQueueId);
                command.Parameters.AddWithValue("idItemStatus", (byte)retryQueueItemDbo.Status);
                command.Parameters.AddWithValue("idSeverityLevel", (byte)retryQueueItemDbo.SeverityLevel);
                command.Parameters.AddWithValue("attemptsCount", retryQueueItemDbo.AttemptsCount);
                command.Parameters.AddWithValue("creationDate", retryQueueItemDbo.CreationDate);
                command.Parameters.AddWithValue("lastExecution", retryQueueItemDbo.LastExecution ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("modifiedStatusDate", retryQueueItemDbo.ModifiedStatusDate ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("description", retryQueueItemDbo.Description ?? (object)DBNull.Value);

                return Convert.ToInt64(await command.ExecuteScalarAsync().ConfigureAwait(false));
            }
        }

        public async Task<bool> AnyItemStillActiveAsync(IDbConnection dbConnection, Guid domainRetryQueueId)
        {
            Guard.Argument(dbConnection).NotNull();
            Guard.Argument(domainRetryQueueId).NotDefault();

            using (var command = dbConnection.CreateCommand())
            {
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = @"SELECT 1 WHERE EXISTS(
                                        SELECT * FROM retry_queue_items
                                        WHERE IdDomainRetryQueue = @IdDomainRetryQueue
                                        AND IdItemStatus IN (@IdItemStatusWaiting, @IdItemStatusInRetry)
                                        LIMIT 1)";

                command.Parameters.AddWithValue("IdDomainRetryQueue", domainRetryQueueId);
                command.Parameters.AddWithValue("IdItemStatusWaiting", (byte)RetryQueueItemStatus.Waiting);
                command.Parameters.AddWithValue("IdItemStatusInRetry", (byte)RetryQueueItemStatus.InRetry);

                var result = await command.ExecuteScalarAsync().ConfigureAwait(false);

                return result is object;
            }
        }

        public async Task<RetryQueueItemDbo> GetItemAsync(IDbConnection dbConnection, Guid domainId)
        {
            Guard.Argument(dbConnection, nameof(dbConnection)).NotNull();
            Guard.Argument(domainId, nameof(domainId)).NotDefault();

            using (var command = dbConnection.CreateCommand())
            {
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = @"SELECT *
                                        FROM retry_queue_items
                                        WHERE IdDomain = @IdDomain";

                command.Parameters.AddWithValue("IdDomain", domainId);

                return await this.ExecuteSingleLineReaderAsync(command).ConfigureAwait(false);
            }
        }

        public async Task<IList<RetryQueueItemDbo>> GetItemsByQueueOrderedAsync(IDbConnection dbConnection, Guid domainRetryQueueId)
        {
            Guard.Argument(dbConnection, nameof(dbConnection)).NotNull();
            Guard.Argument(domainRetryQueueId, nameof(domainRetryQueueId)).NotDefault();

            using (var command = dbConnection.CreateCommand())
            {
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = @"SELECT *
                                        FROM retry_queue_items
                                        WHERE IdDomainRetryQueue = @IdDomainRetryQueue
                                        ORDER BY Sort ASC";

                command.Parameters.AddWithValue("IdDomainRetryQueue", domainRetryQueueId);

                return await this.ExecuteReaderAsync(command).ConfigureAwait(false);
            }
        }

        public async Task<IList<RetryQueueItemDbo>> GetItemsOrderedAsync(
            IDbConnection dbConnection,
            IEnumerable<Guid> retryQueueIds,
            IEnumerable<RetryQueueItemStatus> statuses,
            IEnumerable<SeverityLevel> severities,
            int? top = null,
            StuckStatusFilter stuckStatusFilter = null)
        {
            Guard.Argument(dbConnection, nameof(dbConnection)).NotNull();
            Guard.Argument(retryQueueIds).NotNull();
            Guard.Argument(statuses, nameof(statuses)).NotNull();

            using (var command = dbConnection.CreateCommand())
            {
                command.CommandType = System.Data.CommandType.Text;

                string query = $@"SELECT ";

                query = string.Concat(query, $@" *
                                    FROM retry_queue_items
                                    WHERE IdDomainRetryQueue IN ({string.Join(",", retryQueueIds.Select(x => $"'{x}'"))})");

                if (stuckStatusFilter is null)
                {
                    query = string.Concat(query, $" AND IdItemStatus IN({ string.Join(",", statuses.Select(x => (byte)x))})");
                }
                else
                {
                    query = string.Concat(query, $@" AND(
                                        IdItemStatus IN({ string.Join(",", statuses.Select(x => (byte)x))})
                                        OR(
                                            IdItemStatus = { (byte)stuckStatusFilter.ItemStatus}
                                            AND ModifiedStatusDate + interval '{Math.Floor(stuckStatusFilter.ExpirationInterval.TotalSeconds)} second' < @DateTimeUtcNow
                                            )
                                        )");
                }

                if (severities is object && severities.Any())
                {
                    query = string.Concat(query, $" AND (IdSeverityLevel IN ({string.Join(",", severities.Select(x => (byte)x))}))");
                }

                query = string.Concat(query, " ORDER BY IdRetryQueue, Id");

                if (top is object)
                {
                    query = string.Concat(query, $" LIMIT {top}");
                }

                command.CommandText = query;
                command.Parameters.AddWithValue("DateTimeUtcNow", DateTime.UtcNow);

                return await this.ExecuteReaderAsync(command).ConfigureAwait(false);
            }
        }

        public async Task<IList<RetryQueueItemDbo>> GetNewestItemsAsync(IDbConnection dbConnection, Guid queueIdDomain, int sort)
        {
            Guard.Argument(queueIdDomain, nameof(queueIdDomain)).NotDefault();
            Guard.Argument(sort, nameof(sort)).NotNegative();

            using (var command = dbConnection.CreateCommand())
            {
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = $@"SELECT *
                                         FROM retry_queue_items
                                         WHERE IdDomainRetryQueue = @IdDomainRetryQueue
                                         AND IdItemStatus IN (@IdItemStatusWaiting, @IdItemStatusInRetry)
                                         AND Sort > @Sort
                                         ORDER BY Sort ASC";

                command.Parameters.AddWithValue("IdDomainRetryQueue", queueIdDomain);
                command.Parameters.AddWithValue("IdItemStatusWaiting", (byte)RetryQueueItemStatus.Waiting);
                command.Parameters.AddWithValue("IdItemStatusInRetry", (byte)RetryQueueItemStatus.InRetry);
                command.Parameters.AddWithValue("Sort", sort);

                return await this.ExecuteReaderAsync(command).ConfigureAwait(false);
            }
        }

        public async Task<IList<RetryQueueItemDbo>> GetPendingItemsAsync(IDbConnection dbConnection, Guid queueIdDomain, int sort)
        {
            Guard.Argument(queueIdDomain, nameof(queueIdDomain)).NotDefault();
            Guard.Argument(sort, nameof(sort)).NotNegative();

            using (var command = dbConnection.CreateCommand())
            {
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = $@"SELECT *
                                         FROM retry_queue_items
                                         WHERE IdDomainRetryQueue = @IdDomainRetryQueue
                                         AND IdItemStatus IN (@IdItemStatusWaiting, @IdItemStatusInRetry)
                                         AND Sort < @Sort
                                         ORDER BY Sort ASC";

                command.Parameters.AddWithValue("IdDomainRetryQueue", queueIdDomain);
                command.Parameters.AddWithValue("IdItemStatusWaiting", (byte)RetryQueueItemStatus.Waiting);
                command.Parameters.AddWithValue("IdItemStatusInRetry", (byte)RetryQueueItemStatus.InRetry);
                command.Parameters.AddWithValue("Sort", sort);

                return await this.ExecuteReaderAsync(command).ConfigureAwait(false);
            }
        }

        public async Task<bool> IsFirstWaitingInQueueAsync(IDbConnection dbConnection, RetryQueueItemDbo item)
        {
            var sortedItems = await this.GetItemsOrderedAsync(
                    dbConnection,
                    new Guid[] { item.DomainRetryQueueId },
                    new RetryQueueItemStatus[] { RetryQueueItemStatus.Waiting },
                    null,
                    1)
                    .ConfigureAwait(false);

            if (sortedItems.Any() && item.Id == sortedItems.First().Id)
            {
                return true;
            }

            return false;
        }

        public async Task<int> UpdateAsync(IDbConnection dbConnection, Guid idDomain, RetryQueueItemStatus status, int attemptsCount, DateTime lastExecution, string description)
        {
            using (var command = dbConnection.CreateCommand())
            {
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = @"UPDATE retry_queue_items
                                        SET IdItemStatus = @IdItemStatus,
                                            AttemptsCount = @AttemptsCount,
                                            LastExecution = @LastExecution,
                                            Description = COALESCE(@Description, Description),
                                            ModifiedStatusDate = @DateTimeUtcNow
                                        WHERE IdDomain = @IdDomain";

                command.Parameters.AddWithValue("IdDomain", idDomain);
                command.Parameters.AddWithValue("IdItemStatus", (byte)status);
                command.Parameters.AddWithValue("AttemptsCount", attemptsCount);
                command.Parameters.AddWithValue("LastExecution", lastExecution);
                command.Parameters.AddWithValue("DateTimeUtcNow", DateTime.UtcNow);
                command.Parameters.AddWithValue("Description", string.IsNullOrEmpty(description) ? (object)DBNull.Value : description);

                return await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        public async Task<int> UpdateStatusAsync(IDbConnection dbConnection, Guid idDomain, RetryQueueItemStatus status)
        {
            Guard.Argument(dbConnection, nameof(dbConnection)).NotNull();
            Guard.Argument(idDomain).NotDefault();
            Guard.Argument(status, nameof(status)).NotDefault();

            using (var command = dbConnection.CreateCommand())
            {
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = @"UPDATE retry_queue_items
                                        SET IdItemStatus = @IdItemStatus,
                                            ModifiedStatusDate = @DateTimeUtcNow
                                        WHERE IdDomain = @IdDomain";

                command.Parameters.AddWithValue("IdItemStatus", (byte)status);
                command.Parameters.AddWithValue("IdDomain", idDomain);
                command.Parameters.AddWithValue("DateTimeUtcNow", DateTime.UtcNow);

                return await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        private async Task<IList<RetryQueueItemDbo>> ExecuteReaderAsync(NpgsqlCommand command)
        {
            var items = new List<RetryQueueItemDbo>();

            using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
            {
                while (await reader.ReadAsync())
                {
                    items.Add(this.FillDbo(reader));
                }
            }

            return items;
        }

        private async Task<RetryQueueItemDbo> ExecuteSingleLineReaderAsync(NpgsqlCommand command)
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

        private RetryQueueItemDbo FillDbo(NpgsqlDataReader reader)
        {
            var lastExecutionOrdinal = reader.GetOrdinal("LastExecution");
            var modifiedStatusDateOrdinal = reader.GetOrdinal("ModifiedStatusDate");
            var descriptionOrdinal = reader.GetOrdinal("Description");

            return new RetryQueueItemDbo
            {
                Id = reader.GetInt64(reader.GetOrdinal("Id")),
                IdDomain = reader.GetGuid(reader.GetOrdinal("IdDomain")),
                RetryQueueId = reader.GetInt64(reader.GetOrdinal("IdRetryQueue")),
                DomainRetryQueueId = reader.GetGuid(reader.GetOrdinal("IdDomainRetryQueue")),
                CreationDate = reader.GetDateTime(reader.GetOrdinal("CreationDate")),
                LastExecution = reader.IsDBNull(lastExecutionOrdinal) ? null : (DateTime?)reader.GetDateTime(lastExecutionOrdinal),
                ModifiedStatusDate = reader.IsDBNull(modifiedStatusDateOrdinal) ? null : (DateTime?)reader.GetDateTime(modifiedStatusDateOrdinal),
                AttemptsCount = reader.GetInt32(reader.GetOrdinal("AttemptsCount")),
                Sort = reader.GetInt32(reader.GetOrdinal("Sort")),
                Status = (RetryQueueItemStatus)reader.GetByte(reader.GetOrdinal("IdItemStatus")),
                SeverityLevel = (SeverityLevel)reader.GetByte(reader.GetOrdinal("IdSeverityLevel")),
                Description = reader.IsDBNull(descriptionOrdinal) ? null : reader.GetString(descriptionOrdinal)
            };
        }
    }
}