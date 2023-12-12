using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using KafkaFlow.Retry.Durable.Common;
using KafkaFlow.Retry.Durable.Repository;
using KafkaFlow.Retry.Durable.Repository.Model;
using KafkaFlow.Retry.SqlServer;
using KafkaFlow.Retry.SqlServer.Model;
using KafkaFlow.Retry.SqlServer.Readers;
using KafkaFlow.Retry.SqlServer.Readers.Adapters;
using KafkaFlow.Retry.SqlServer.Repositories;

namespace KafkaFlow.Retry.IntegrationTests.Core.Storages.Repositories;

internal class SqlServerRepository : IRepository
{
    private const string Schema = "dbo";
    private const int TimeoutSec = 60;
    private readonly ConnectionProvider _connectionProvider;
    private readonly IRetryQueueItemMessageHeaderRepository _retryQueueItemMessageHeaderRepository;
    private readonly IRetryQueueItemMessageRepository _retryQueueItemMessageRepository;
    private readonly IRetryQueueItemRepository _retryQueueItemRepository;
    private readonly RetryQueueReader _retryQueueReader;
    private readonly IRetryQueueRepository _retryQueueRepository;
    private readonly SqlServerDbSettings _sqlServerDbSettings;

    public SqlServerRepository(
        string connectionString,
        string dbName)
    {
        _sqlServerDbSettings = new SqlServerDbSettings(connectionString, dbName, Schema);

        RetryQueueDataProvider = new SqlServerDbDataProviderFactory().Create(_sqlServerDbSettings);

        _retryQueueItemMessageHeaderRepository = new RetryQueueItemMessageHeaderRepository();
        _retryQueueItemMessageRepository = new RetryQueueItemMessageRepository();
        _retryQueueItemRepository = new RetryQueueItemRepository();
        _retryQueueRepository = new RetryQueueRepository();

        _retryQueueReader = new RetryQueueReader(
            new RetryQueueAdapter(),
            new RetryQueueItemAdapter(),
            new RetryQueueItemMessageAdapter(),
            new RetryQueueItemMessageHeaderAdapter()
        );

        _connectionProvider = new ConnectionProvider();
    }

    public RepositoryType RepositoryType => RepositoryType.SqlServer;

    public IRetryDurableQueueRepositoryProvider RetryQueueDataProvider { get; }

    public async Task CleanDatabaseAsync()
    {
        using var dbConnection = _connectionProvider.Create(_sqlServerDbSettings);
        using var command = dbConnection.CreateCommand();
        command.CommandType = System.Data.CommandType.Text;
        command.CommandText = @"
                    delete from [dbo].[RetryItemMessageHeaders];
                    delete from [dbo].[ItemMessages];
                    delete from [dbo].[RetryQueues];
                    delete from [dbo].[RetryQueueItems];
                ";
        await command.ExecuteNonQueryAsync();
    }

    public async Task CreateQueueAsync(RetryQueue queue)
    {
        var queueDbo = new RetryQueueDbo
        {
            IdDomain = queue.Id,
            CreationDate = queue.CreationDate,
            LastExecution = queue.LastExecution,
            QueueGroupKey = queue.QueueGroupKey,
            SearchGroupKey = queue.SearchGroupKey,
            Status = queue.Status,
        };

        using var dbConnection = _connectionProvider.CreateWithinTransaction(_sqlServerDbSettings);

        var queueId = await _retryQueueRepository.AddAsync(dbConnection, queueDbo);

        foreach (var item in queue.Items)
        {
            // queue item
            var itemDbo = new RetryQueueItemDbo
            {
                IdDomain = item.Id,
                CreationDate = item.CreationDate,
                LastExecution = item.LastExecution,
                ModifiedStatusDate = item.ModifiedStatusDate,
                AttemptsCount = item.AttemptsCount,
                RetryQueueId = queueId,
                DomainRetryQueueId = queue.Id,
                Status = item.Status,
                SeverityLevel = item.SeverityLevel,
                Description = item.Description
            };

            var itemId = await _retryQueueItemRepository.AddAsync(dbConnection, itemDbo);

            // item message
            var messageDbo = new RetryQueueItemMessageDbo
            {
                IdRetryQueueItem = itemId,
                Key = item.Message.Key,
                Offset = item.Message.Offset,
                Partition = item.Message.Partition,
                TopicName = item.Message.TopicName,
                UtcTimeStamp = item.Message.UtcTimeStamp,
                Value = item.Message.Value
            };

            await _retryQueueItemMessageRepository.AddAsync(dbConnection, messageDbo);

            // message headers
            var messageHeadersDbos = item.Message.Headers
                .Select(h => new RetryQueueItemMessageHeaderDbo
                {
                    RetryQueueItemMessageId = itemId,
                    Key = h.Key,
                    Value = h.Value
                });

            await _retryQueueItemMessageHeaderRepository.AddAsync(dbConnection, messageHeadersDbos);
        }

        dbConnection.Commit();
    }

    public async Task<RetryQueue> GetAllRetryQueueDataAsync(string queueGroupKey)
    {
        using (var dbConnection = _connectionProvider.Create(_sqlServerDbSettings))
        {
            var retryQueueDbo = await _retryQueueRepository.GetQueueAsync(dbConnection, queueGroupKey);

            if (retryQueueDbo is null)
            {
                return null;
            }

            var retryQueueItemsDbo = await _retryQueueItemRepository.GetItemsByQueueOrderedAsync(dbConnection, retryQueueDbo.IdDomain);
            var itemMessagesDbo = await _retryQueueItemMessageRepository.GetMessagesOrderedAsync(dbConnection, retryQueueItemsDbo);
            var messageHeadersDbo = await _retryQueueItemMessageHeaderRepository.GetOrderedAsync(dbConnection, itemMessagesDbo);

            var dboWrapper = new RetryQueuesDboWrapper
            {
                QueuesDbos = new[] { retryQueueDbo },
                ItemsDbos = retryQueueItemsDbo,
                MessagesDbos = itemMessagesDbo,
                HeadersDbos = messageHeadersDbo
            };

            return _retryQueueReader.Read(dboWrapper).FirstOrDefault();
        }
    }

    public async Task<RetryQueue> GetRetryQueueAsync(string queueGroupKey)
    {
        var start = DateTime.Now;
        Guid retryQueueId = Guid.Empty;
        RetryQueue retryQueue;
        do
        {
            if (DateTime.Now.Subtract(start).TotalSeconds > TimeoutSec && !Debugger.IsAttached)
            {
                return null;
            }

            await Task.Delay(100);

            using (var dbConnection = _connectionProvider.Create(_sqlServerDbSettings))
            using (var command = dbConnection.CreateCommand())
            {
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = @"SELECT Id, IdDomain, IdStatus, SearchGroupKey, QueueGroupKey, CreationDate, LastExecution
                                FROM [RetryQueues]
                                WHERE QueueGroupKey LIKE '%'+@QueueGroupKey
                                ORDER BY Id";

                command.Parameters.AddWithValue("QueueGroupKey", queueGroupKey);
                retryQueue = await ExecuteSingleLineReaderAsync(command);
            }

            if (retryQueue != null)
            {
                retryQueueId = retryQueue.Id;
            }
        } while (retryQueueId == Guid.Empty);

        return retryQueue;
    }

    public async Task<IList<RetryQueueItem>> GetRetryQueueItemsAsync(Guid retryQueueId, Func<IList<RetryQueueItem>, bool> stopCondition)
    {
        var start = DateTime.Now;
        IList<RetryQueueItem> retryQueueItems = null;
        do
        {
            if (DateTime.Now.Subtract(start).TotalSeconds > TimeoutSec && !Debugger.IsAttached)
            {
                return null;
            }

            await Task.Delay(100);

            using (var dbConnection = _connectionProvider.Create(_sqlServerDbSettings))
            using (var command = dbConnection.CreateCommand())
            {
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = @"SELECT *
                                FROM [RetryQueueItems]
                                WHERE IdDomainRetryQueue = @IdDomainRetryQueue
                                ORDER BY Sort ASC";

                command.Parameters.AddWithValue("IdDomainRetryQueue", retryQueueId);
                retryQueueItems = await ExecuteReaderAsync(command);
            }
        } while (stopCondition(retryQueueItems));

        return retryQueueItems ?? new List<RetryQueueItem>();
    }

    private async Task<IList<RetryQueueItem>> ExecuteReaderAsync(SqlCommand command)
    {
        var items = new List<RetryQueueItem>();

        using (var reader = await command.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                items.Add(FillRetryQueueItem(reader));
            }
        }

        return items;
    }

    private async Task<RetryQueue> ExecuteSingleLineReaderAsync(SqlCommand command)
    {
        using (var reader = await command.ExecuteReaderAsync())
        {
            if (await reader.ReadAsync())
            {
                return FillRetryQueue(reader);
            }
        }

        return null;
    }

    private RetryQueue FillRetryQueue(SqlDataReader reader)
    {
        return new RetryQueue(
            reader.GetGuid(reader.GetOrdinal("IdDomain")),
            reader.GetString(reader.GetOrdinal("SearchGroupKey")),
            reader.GetString(reader.GetOrdinal("QueueGroupKey")),
            reader.GetDateTime(reader.GetOrdinal("CreationDate")),
            reader.GetDateTime(reader.GetOrdinal("LastExecution")),
            (RetryQueueStatus)reader.GetByte(reader.GetOrdinal("IdStatus"))
        );
    }

    private RetryQueueItem FillRetryQueueItem(SqlDataReader reader)
    {
        var lastExecutionOrdinal = reader.GetOrdinal("LastExecution");
        var modifiedStatusDateOrdinal = reader.GetOrdinal("ModifiedStatusDate");
        var descriptionOrdinal = reader.GetOrdinal("Description");

        return new RetryQueueItem(
            reader.GetGuid(reader.GetOrdinal("IdDomain")),
            reader.GetInt32(reader.GetOrdinal("AttemptsCount")),
            reader.GetDateTime(reader.GetOrdinal("CreationDate")),
            reader.GetInt32(reader.GetOrdinal("Sort")),
            reader.IsDBNull(lastExecutionOrdinal) ? null : (DateTime?)reader.GetDateTime(lastExecutionOrdinal),
            reader.IsDBNull(modifiedStatusDateOrdinal) ? null : (DateTime?)reader.GetDateTime(modifiedStatusDateOrdinal),
            (RetryQueueItemStatus)reader.GetByte(reader.GetOrdinal("IdItemStatus")),
            (SeverityLevel)reader.GetByte(reader.GetOrdinal("IdSeverityLevel")),
            reader.IsDBNull(descriptionOrdinal) ? null : reader.GetString(descriptionOrdinal)
        );
    }
}