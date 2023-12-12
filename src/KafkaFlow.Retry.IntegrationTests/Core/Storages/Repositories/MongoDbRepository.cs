using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dawn;
using KafkaFlow.Retry.Durable.Repository;
using KafkaFlow.Retry.Durable.Repository.Model;
using KafkaFlow.Retry.MongoDb;
using KafkaFlow.Retry.MongoDb.Adapters;
using KafkaFlow.Retry.MongoDb.Model;
using MongoDB.Driver;

namespace KafkaFlow.Retry.IntegrationTests.Core.Storages.Repositories;

internal class MongoDbRepository : IRepository
{
    private const int TimeoutSec = 60;
    private readonly string _databaseName;

    private readonly MongoClient _mongoClient;
    private readonly QueuesAdapter _queuesAdapter;
    private readonly IMongoCollection<RetryQueueItemDbo> _retryQueueItemsCollection;
    private readonly IMongoCollection<RetryQueueDbo> _retryQueuesCollection;

    public MongoDbRepository(
        string connectionString,
        string dbName,
        string retryQueueCollectionName,
        string retryQueueItemCollectionName)
    {
        _databaseName = dbName;
        _mongoClient = new MongoClient(connectionString);
        _retryQueuesCollection = _mongoClient.GetDatabase(dbName).GetCollection<RetryQueueDbo>(retryQueueCollectionName);
        _retryQueueItemsCollection = _mongoClient.GetDatabase(dbName).GetCollection<RetryQueueItemDbo>(retryQueueItemCollectionName);

        var dataProviderCreationResult = new MongoDbDataProviderFactory().TryCreate(
            new MongoDbSettings
            {
                ConnectionString = connectionString,
                DatabaseName = dbName,
                RetryQueueCollectionName = retryQueueCollectionName,
                RetryQueueItemCollectionName = retryQueueItemCollectionName
            });

        _queuesAdapter =
            new QueuesAdapter(
                new ItemAdapter(
                    new MessageAdapter(
                        new HeaderAdapter())));

        Guard.Argument(dataProviderCreationResult, nameof(dataProviderCreationResult)).NotNull();
        Guard.Argument(dataProviderCreationResult.Success, nameof(dataProviderCreationResult.Success)).True(dataProviderCreationResult.Message);

        RetryQueueDataProvider = dataProviderCreationResult.Result;
    }

    public RepositoryType RepositoryType => RepositoryType.MongoDb;

    public IRetryDurableQueueRepositoryProvider RetryQueueDataProvider { get; }

    public async Task CleanDatabaseAsync()
    {
        await _mongoClient.DropDatabaseAsync(_databaseName).ConfigureAwait(false);
    }

    public async Task CreateQueueAsync(RetryQueue queue)
    {
        var queueDbo = new RetryQueueDbo
        {
            Id = queue.Id,
            CreationDate = queue.CreationDate,
            LastExecution = queue.LastExecution,
            QueueGroupKey = queue.QueueGroupKey,
            SearchGroupKey = queue.SearchGroupKey,
            Status = queue.Status,
        };

        await _retryQueuesCollection.InsertOneAsync(queueDbo);

        foreach (var item in queue.Items)
        {
            var itemDbo = new RetryQueueItemDbo
            {
                Id = item.Id,
                CreationDate = item.CreationDate,
                LastExecution = item.LastExecution,
                ModifiedStatusDate = item.ModifiedStatusDate,
                AttemptsCount = item.AttemptsCount,
                RetryQueueId = queue.Id,
                Status = item.Status,
                SeverityLevel = item.SeverityLevel,
                Description = item.Description,
                Message = new RetryQueueItemMessageDbo
                {
                    Headers = item.Message.Headers
                        .Select(h => new RetryQueueHeaderDbo
                        {
                            Key = h.Key,
                            Value = h.Value
                        }),
                    Key = item.Message.Key,
                    Offset = item.Message.Offset,
                    Partition = item.Message.Partition,
                    TopicName = item.Message.TopicName,
                    UtcTimeStamp = item.Message.UtcTimeStamp,
                    Value = item.Message.Value
                },
                Sort = item.Sort
            };

            await _retryQueueItemsCollection.InsertOneAsync(itemDbo);
        }
    }

    public async Task<RetryQueue> GetAllRetryQueueDataAsync(string queueGroupKey)
    {
        var queueCursor = await _retryQueuesCollection.FindAsync(x => x.QueueGroupKey == queueGroupKey);

        var queue = await queueCursor.FirstOrDefaultAsync();

        if (queue is null)
        {
            return null;
        }

        var itemsCursor = await _retryQueueItemsCollection.FindAsync(x => x.RetryQueueId == queue.Id);

        var items = await itemsCursor.ToListAsync();

        return _queuesAdapter.Adapt(new[] { queue }, items).First();
    }

    public async Task<RetryQueue> GetRetryQueueAsync(string queueGroupKey)
    {
        var start = DateTime.Now;
        Guid retryQueueId = Guid.Empty;
        RetryQueueDbo retryQueueDbo = new RetryQueueDbo();
        do
        {
            if (DateTime.Now.Subtract(start).TotalSeconds > TimeoutSec && !Debugger.IsAttached)
            {
                return null;
            }

            await Task.Delay(100).ConfigureAwait(false);

            var retryQueueCursor = await _retryQueuesCollection.FindAsync(x => x.QueueGroupKey.Contains(queueGroupKey)).ConfigureAwait(false);
            var retryQueues = await retryQueueCursor.ToListAsync().ConfigureAwait(false);
            if (retryQueues.Any())
            {
                retryQueueDbo = retryQueues.Single();
                retryQueueId = retryQueueDbo.Id;
            }
        } while (retryQueueId == Guid.Empty);

        return new RetryQueue(
            retryQueueDbo.Id,
            retryQueueDbo.SearchGroupKey,
            retryQueueDbo.QueueGroupKey,
            retryQueueDbo.CreationDate,
            retryQueueDbo.LastExecution,
            retryQueueDbo.Status);
    }

    public async Task<IList<RetryQueueItem>> GetRetryQueueItemsAsync(
        Guid retryQueueId,
        Func<IList<RetryQueueItem>, bool> stopCondition)
    {
        var start = DateTime.Now;
        List<RetryQueueItem> retryQueueItems = null;
        do
        {
            if (DateTime.Now.Subtract(start).TotalSeconds > TimeoutSec && !Debugger.IsAttached)
            {
                return null;
            }

            await Task.Delay(100).ConfigureAwait(false);

            var retryQueueItemsCursor = await _retryQueueItemsCollection.FindAsync(x => x.RetryQueueId == retryQueueId).ConfigureAwait(false);
            var retryQueueItemsDbo = await retryQueueItemsCursor
                .ToListAsync()
                .ConfigureAwait(false);

            retryQueueItems = retryQueueItemsDbo
                .Select(
                    x =>
                    {
                        return new RetryQueueItem(
                            x.Id,
                            x.AttemptsCount,
                            x.CreationDate,
                            x.Sort,
                            x.LastExecution,
                            x.ModifiedStatusDate,
                            x.Status,
                            x.SeverityLevel,
                            x.Description);
                    }).ToList();
        } while (stopCondition(retryQueueItems));

        return retryQueueItems ?? new List<RetryQueueItem>();
    }
}