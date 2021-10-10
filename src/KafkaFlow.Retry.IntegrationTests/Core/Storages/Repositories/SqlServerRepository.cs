namespace KafkaFlow.Retry.IntegrationTests.Core.Storages.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Threading.Tasks;
    using KafkaFlow.Retry.IntegrationTests.Core.Messages;
    using KafkaFlow.Retry.IntegrationTests.Core.Storages.Models;

    internal class SqlServerRepository : IRepository
    {
        private const int TimeoutSec = 60;
        private readonly string connectionString;
        private readonly string dbName;
        private SqlConnection sqlConnection;

        public SqlServerRepository(
            string connectionString,
            string dbName)
        {
            this.connectionString = connectionString;
            this.dbName = dbName;
        }

        public Type RepositoryType => typeof(SqlServerRepository);

        public async Task CleanDatabaseAsync()
        {
            using (var command = CreateCommand())
            {
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = @"
                    delete from [dbo].[RetryItemMessageHeaders];
                    delete from [dbo].[ItemMessages];
                    delete from [dbo].[RetryQueues];
                    delete from [dbo].[RetryQueueItems];
                ";
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        public void Dispose()
        {
            if (this.sqlConnection is object)
            {
                this.sqlConnection.Dispose();
            }
        }

        public async Task<RetryQueueTestModel> GetRetryQueueAsync(RetryDurableTestMessage message)
        {
            var start = DateTime.Now;
            Guid retryQueueId = Guid.Empty;
            RetryQueueTestModel retryQueue = new RetryQueueTestModel();
            do
            {
                if (DateTime.Now.Subtract(start).Seconds > TimeoutSec)
                {
                    return retryQueue;
                }

                await Task.Delay(100).ConfigureAwait(false);

                using (var command = CreateCommand())
                {
                    command.CommandType = System.Data.CommandType.Text;
                    command.CommandText = @"SELECT Id, IdDomain, IdStatus, SearchGroupKey, QueueGroupKey, CreationDate, LastExecution
                                FROM [RetryQueues]
                                WHERE QueueGroupKey = @QueueGroupKey
                                ORDER BY Id";

                    command.Parameters.AddWithValue("QueueGroupKey", message.Key);
                    retryQueue = await this.ExecuteSingleLineReaderAsync(command).ConfigureAwait(false);
                }

                if (retryQueue != null)
                {
                    retryQueueId = retryQueue.Id;
                }
            } while (retryQueueId == Guid.Empty);

            return retryQueue;
        }

        public async Task<IList<RetryQueueItemTestModel>> GetRetryQueueItemsAsync(Guid retryQueueId, Func<IList<RetryQueueItemTestModel>, bool> stopCondition)
        {
            var start = DateTime.Now;
            IList<RetryQueueItemTestModel> retryQueueItems = new List<RetryQueueItemTestModel>();
            do
            {
                if (DateTime.Now.Subtract(start).Seconds > TimeoutSec)
                {
                    return null;
                }

                await Task.Delay(100).ConfigureAwait(false);
                using (var command = CreateCommand())
                {
                    command.CommandType = System.Data.CommandType.Text;
                    command.CommandText = @"SELECT *
                                FROM [RetryQueueItems]
                                WHERE IdDomainRetryQueue = @IdDomainRetryQueue
                                ORDER BY Sort ASC";

                    command.Parameters.AddWithValue("IdDomainRetryQueue", retryQueueId);
                    retryQueueItems = await this.ExecuteReaderAsync(command).ConfigureAwait(false);
                }
            } while (stopCondition(retryQueueItems));

            return retryQueueItems;
        }

        private SqlCommand CreateCommand() => this.GetDbConnection().CreateCommand();

        private async Task<IList<RetryQueueItemTestModel>> ExecuteReaderAsync(SqlCommand command)
        {
            var items = new List<RetryQueueItemTestModel>();

            using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    items.Add(this.FillRetryQueueItemTestModel(reader));
                }
            }

            return items;
        }

        private async Task<RetryQueueTestModel> ExecuteSingleLineReaderAsync(SqlCommand command)
        {
            using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
            {
                if (await reader.ReadAsync().ConfigureAwait(false))
                {
                    return this.FillRetryQueueTestModel(reader);
                }
            }

            return null;
        }

        private RetryQueueItemTestModel FillRetryQueueItemTestModel(SqlDataReader reader)
        {
            var lastExecutionOrdinal = reader.GetOrdinal("LastExecution");
            var modifiedStatusDateOrdinal = reader.GetOrdinal("ModifiedStatusDate");
            var descriptionOrdinal = reader.GetOrdinal("Description");

            return new RetryQueueItemTestModel
            {
                Id = reader.GetGuid(reader.GetOrdinal("IdDomain")),
                RetryQueueId = reader.GetGuid(reader.GetOrdinal("IdDomainRetryQueue")),
                CreationDate = reader.GetDateTime(reader.GetOrdinal("CreationDate")),
                LastExecution = reader.IsDBNull(lastExecutionOrdinal) ? null : (DateTime?)reader.GetDateTime(lastExecutionOrdinal),
                ModifiedStatusDate = reader.IsDBNull(modifiedStatusDateOrdinal) ? null : (DateTime?)reader.GetDateTime(modifiedStatusDateOrdinal),
                AttemptsCount = reader.GetInt32(reader.GetOrdinal("AttemptsCount")),
                Sort = reader.GetInt32(reader.GetOrdinal("Sort")),
                Status = (RetryQueueItemStatusTestModel)reader.GetByte(reader.GetOrdinal("IdItemStatus")),
                Description = reader.IsDBNull(descriptionOrdinal) ? null : reader.GetString(descriptionOrdinal)
            };
        }

        private RetryQueueTestModel FillRetryQueueTestModel(SqlDataReader reader)
        {
            return new RetryQueueTestModel
            {
                Id = reader.GetGuid(reader.GetOrdinal("IdDomain")),
                CreationDate = reader.GetDateTime(reader.GetOrdinal("CreationDate")),
                LastExecution = reader.GetDateTime(reader.GetOrdinal("LastExecution")),
                QueueGroupKey = reader.GetString(reader.GetOrdinal("QueueGroupKey")),
                SearchGroupKey = reader.GetString(reader.GetOrdinal("SearchGroupKey")),
                Status = (RetryQueueStatusTestModel)reader.GetByte(reader.GetOrdinal("IdStatus"))
            };
        }

        private SqlConnection GetDbConnection()
        {
            if (this.sqlConnection is null)
            {
                this.sqlConnection = new SqlConnection(this.connectionString);
                this.sqlConnection.Open();
                this.sqlConnection.ChangeDatabase(this.dbName);
            }

            return this.sqlConnection;
        }
    }
}