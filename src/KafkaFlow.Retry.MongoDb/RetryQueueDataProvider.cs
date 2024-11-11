using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dawn;
using KafkaFlow.Retry.Durable.Repository;
using KafkaFlow.Retry.Durable.Repository.Actions.Create;
using KafkaFlow.Retry.Durable.Repository.Actions.Delete;
using KafkaFlow.Retry.Durable.Repository.Actions.Read;
using KafkaFlow.Retry.Durable.Repository.Actions.Update;
using KafkaFlow.Retry.Durable.Repository.Model;
using KafkaFlow.Retry.MongoDb.Adapters;
using KafkaFlow.Retry.MongoDb.Adapters.Interfaces;
using KafkaFlow.Retry.MongoDb.Model;
using KafkaFlow.Retry.MongoDb.Model.Factories;
using KafkaFlow.Retry.MongoDb.Repositories;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace KafkaFlow.Retry.MongoDb;

internal sealed class RetryQueueDataProvider : IRetryDurableQueueRepositoryProvider
{
    private readonly DbContext _dbContext;
    private readonly IQueuesAdapter _queuesAdapter;
    private readonly RetryQueueItemDboFactory _retryQueueItemDboFactory;
    private readonly IRetryQueueItemRepository _retryQueueItemRepository;
    private readonly IRetryQueueRepository _retryQueueRepository;

    internal RetryQueueDataProvider(
        DbContext dbContext,
        IRetryQueueRepository retryQueueRepository,
        IRetryQueueItemRepository retryQueueItemRepository)
    {
        Guard.Argument(dbContext).NotNull();

        _dbContext = dbContext;
        _retryQueueRepository = retryQueueRepository;
        _retryQueueItemRepository = retryQueueItemRepository;
        var messageAdapter = new MessageAdapter(new HeaderAdapter());

        _retryQueueItemDboFactory = new RetryQueueItemDboFactory(messageAdapter);
        _queuesAdapter = new QueuesAdapter(new ItemAdapter(messageAdapter));
    }

    public async Task<CheckQueueResult> CheckQueueAsync(CheckQueueInput input)
    {
        Guard.Argument(input).NotNull();

        // Tries to find an active queue for the GroupKey
        var retryQueueDbo = await _dbContext.RetryQueues
            .AsQueryable()
            .FirstOrDefaultAsync(q =>
                q.QueueGroupKey == input.QueueGroupKey &&
                q.Status != RetryQueueStatus.Done
            ).ConfigureAwait(false);

        if (retryQueueDbo != null)
        {
            return new CheckQueueResult(CheckQueueResultStatus.Exists);
        }

        return new CheckQueueResult(CheckQueueResultStatus.DoesNotExist);
    }

    public async Task<QueueNewestItemsResult> CheckQueueNewestItemsAsync(QueueNewestItemsInput input)
    {
        Guard.Argument(input, nameof(input)).NotNull();

        var itemsFilterBuilder = _dbContext.RetryQueueItems.GetFilters();

        var itemsFilter = itemsFilterBuilder.Eq(i => i.RetryQueueId, input.QueueId)
                          & itemsFilterBuilder.In(i => i.Status,
                              new[] { RetryQueueItemStatus.Waiting, RetryQueueItemStatus.InRetry })
                          & itemsFilterBuilder.Gt(i => i.Sort, input.Sort);

        var itemsDbo = await _dbContext.RetryQueueItems.GetAsync(itemsFilter).ConfigureAwait(false);

        if (itemsDbo.Any())
        {
            return new QueueNewestItemsResult(QueueNewestItemsResultStatus.HasNewestItems);
        }

        return new QueueNewestItemsResult(QueueNewestItemsResultStatus.NoNewestItems);
    }

    public async Task<QueuePendingItemsResult> CheckQueuePendingItemsAsync(QueuePendingItemsInput input)
    {
        Guard.Argument(input, nameof(input)).NotNull();

        var itemsFilterBuilder = _dbContext.RetryQueueItems.GetFilters();

        var itemsFilter = itemsFilterBuilder.Eq(i => i.RetryQueueId, input.QueueId)
                          & itemsFilterBuilder.In(i => i.Status,
                              new[] { RetryQueueItemStatus.Waiting, RetryQueueItemStatus.InRetry })
                          & itemsFilterBuilder.Lt(i => i.Sort, input.Sort);

        var itemsDbo = await _dbContext.RetryQueueItems.GetAsync(itemsFilter).ConfigureAwait(false);

        if (itemsDbo.Any())
        {
            return new QueuePendingItemsResult(QueuePendingItemsResultStatus.HasPendingItems);
        }

        return new QueuePendingItemsResult(QueuePendingItemsResultStatus.NoPendingItems);
    }

    public Task<long> CountQueuesAsync(CountQueuesInput input)
    {
        throw new NotImplementedException();
    }

    public async Task<DeleteQueuesResult> DeleteQueuesAsync(DeleteQueuesInput input)
    {
        Guard.Argument(input, nameof(input)).NotNull();

        var queueIdsToDelete = await _retryQueueRepository
            .GetQueuesToDeleteAsync(
                input.SearchGroupKey,
                input.RetryQueueStatus,
                input.MaxLastExecutionDateToBeKept,
                input.MaxRowsToDelete)
            .ConfigureAwait(false);

        await _retryQueueItemRepository
            .DeleteItemsAsync(queueIdsToDelete)
            .ConfigureAwait(false);

        return await _retryQueueRepository
            .DeleteQueuesAsync(queueIdsToDelete)
            .ConfigureAwait(false);
    }

    public async Task<GetQueuesResult> GetQueuesAsync(GetQueuesInput input)
    {
        Guard.Argument(input, nameof(input)).NotNull();

        var queuesDbo = await _retryQueueRepository
            .GetTopSortedQueuesAsync(input.Status, input.SortOption, input.SearchGroupKey, input.TopQueues)
            .ConfigureAwait(false);

        if (!queuesDbo.Any())
        {
            return new GetQueuesResult(Enumerable.Empty<RetryQueue>());
        }

        var itemsDbo = new List<RetryQueueItemDbo>();

        var queueIds = queuesDbo.Select(q => q.Id);

        foreach (var queueId in queueIds)
        {
            var queueeItemsDbo = await _retryQueueItemRepository.GetItemsAsync(
                    new[] { queueId },
                    input.ItemsStatuses,
                    input.SeverityLevels,
                    input.TopItemsByQueue,
                    input.StuckStatusFilter)
                .ConfigureAwait(false);

            itemsDbo.AddRange(queueeItemsDbo);
        }

        var queues = _queuesAdapter.Adapt(queuesDbo, itemsDbo);

        return new GetQueuesResult(queues);
    }

    public async Task<SaveToQueueResult> SaveToQueueAsync(SaveToQueueInput input)
    {
        Guard.Argument(input).NotNull();

        var retryQueueDbo = await _dbContext.RetryQueues
            .AsQueryable()
            .FirstOrDefaultAsync(q => q.QueueGroupKey == input.QueueGroupKey);

        if (retryQueueDbo is null)
        {
            await CreateItemIntoANewQueueAsync(input).ConfigureAwait(false);
            return new SaveToQueueResult(SaveToQueueResultStatus.Created);
        }

        await AddItemIntoAnExistingQueueAsync(input, retryQueueDbo).ConfigureAwait(false);
        return new SaveToQueueResult(SaveToQueueResultStatus.Added);
    }

    public async Task<UpdateItemResult> UpdateItemExecutionInfoAsync(UpdateItemExecutionInfoInput input)
    {
        Guard.Argument(input, nameof(input)).NotNull();

        return await UpdateItemAndTryUpdateQueueToDoneAsync(input).ConfigureAwait(false);
    }

    public async Task<UpdateItemsResult> UpdateItemsAsync(UpdateItemsInput input)
    {
        Guard.Argument(input, nameof(input)).NotNull();

        var results = new List<UpdateItemResult>();

        foreach (var itemId in input.ItemIds)
        {
            var result = await UpdateItemAndQueueStatusAsync(new UpdateItemStatusInput(itemId, input.Status))
                .ConfigureAwait(false);

            results.Add(result);
        }

        return new UpdateItemsResult(results);
    }

    public async Task<UpdateItemResult> UpdateItemStatusAsync(UpdateItemStatusInput input)
    {
        Guard.Argument(input, nameof(input)).NotNull();

        var filter = _dbContext.RetryQueueItems.GetFilters().Eq(i => i.Id, input.ItemId);

        var update = _dbContext.RetryQueueItems.GetUpdateDefinition().Set(i => i.Status, input.Status)
            .Set(i => i.ModifiedStatusDate, DateTime.UtcNow);

        var updateResult = await _dbContext.RetryQueueItems.UpdateOneAsync(filter, update).ConfigureAwait(false);

        if (updateResult.IsAcknowledged && updateResult.MatchedCount == 0)
        {
            return new UpdateItemResult(input.ItemId, UpdateItemResultStatus.ItemNotFound);
        }

        return new UpdateItemResult(input.ItemId, UpdateItemResultStatus.Updated);
    }

    public async Task<UpdateQueuesResult> UpdateQueuesAsync(UpdateQueuesInput input)
    {
        Guard.Argument(input, nameof(input)).NotNull();

        var results = new List<UpdateQueueResult>();

        foreach (var queueGroupKey in input.QueueGroupKeys)
        {
            var result = await UpdateQueueAndAllItemsAsync(new UpdateItemsInQueueInput(queueGroupKey, input.ItemStatus))
                .ConfigureAwait(false);

            results.Add(result);
        }

        return new UpdateQueuesResult(results);
    }

    private async Task AddItemIntoAnExistingQueueAsync(SaveToQueueInput input, RetryQueueDbo retryQueueDbo)
    {
        // Gets the total items in the queue.
        var totalItemsInQueue = _dbContext.RetryQueueItems
            .AsQueryable()
            .Where(i => i.RetryQueueId == retryQueueDbo.Id)
            .Count();

        // Inserts the new item at the last position in the queue.
        var retryQueueItemDbo = _retryQueueItemDboFactory.Create(input, retryQueueDbo.Id, totalItemsInQueue);
        await _dbContext.RetryQueueItems.InsertOneAsync(retryQueueItemDbo).ConfigureAwait(false);

        // Verifies whether to change the queue status.
        if (retryQueueDbo.Status == RetryQueueStatus.Done)
        {
            // The queue was marked as DONE. With this new item, the status should return to ACTIVE.
            await _dbContext.RetryQueues
                .FindOneAndUpdateAsync(
                    q => q.Id == retryQueueDbo.Id,
                    Builders<RetryQueueDbo>.Update.Set(q => q.Status, RetryQueueStatus.Active)
                ).ConfigureAwait(false);
        }
    }

    private async Task CreateItemIntoANewQueueAsync(SaveToQueueInput input)
    {
        // Creates the queue
        var retryQueueDbo = RetryQueueDboFactory.Create(input);
        await _dbContext.RetryQueues.InsertOneAsync(retryQueueDbo).ConfigureAwait(false);

        // Adds the item
        var retryQueueItemDbo = _retryQueueItemDboFactory.Create(input, retryQueueDbo.Id);
        await _dbContext.RetryQueueItems.InsertOneAsync(retryQueueItemDbo).ConfigureAwait(false);
    }

    private bool IsItemInWaitingState(RetryQueueItemDbo item)
    {
        return item.Status == RetryQueueItemStatus.Waiting;
    }

    private async Task<UpdateQueueResultStatus> TryUpdateQueueToDoneAsync(Guid queueId)
    {
        var anyItemStillActive = await _retryQueueItemRepository.AnyItemStillActiveAsync(queueId).ConfigureAwait(false);

        if (!anyItemStillActive)
        {
            var updateQueueResult = await _retryQueueRepository.UpdateStatusAsync(queueId, RetryQueueStatus.Done)
                .ConfigureAwait(false);

            if (updateQueueResult.IsAcknowledged && updateQueueResult.MatchedCount == 0)
            {
                return UpdateQueueResultStatus.QueueNotFound;
            }

            return UpdateQueueResultStatus.Updated;
        }

        return UpdateQueueResultStatus.NotUpdated;
    }

    private async Task<UpdateItemResult> UpdateItemAndQueueStatusAsync(UpdateItemStatusInput input)
    {
        if (input.Status != RetryQueueItemStatus.Cancelled)
        {
            return new UpdateItemResult(input.ItemId, UpdateItemResultStatus.UpdateIsNotAllowed);
        }

        var item = await _retryQueueItemRepository.GetItemAsync(input.ItemId).ConfigureAwait(false);

        if (item is null)
        {
            return new UpdateItemResult(input.ItemId, UpdateItemResultStatus.ItemNotFound);
        }

        if (!IsItemInWaitingState(item))
        {
            return new UpdateItemResult(input.ItemId, UpdateItemResultStatus.ItemIsNotInWaitingState);
        }

        if (!await _retryQueueItemRepository.IsFirstWaitingInQueue(item).ConfigureAwait(false))
        {
            return new UpdateItemResult(input.ItemId, UpdateItemResultStatus.ItemIsNotTheFirstWaitingInQueue);
        }

        var updateItemResult = await UpdateItemStatusAsync(input).ConfigureAwait(false);

        if (updateItemResult.Status == UpdateItemResultStatus.ItemNotFound)
        {
            return updateItemResult;
        }

        var updateQueueResultStatus = await TryUpdateQueueToDoneAsync(item.RetryQueueId).ConfigureAwait(false);

        if (updateQueueResultStatus == UpdateQueueResultStatus.QueueNotFound)
        {
            return new UpdateItemResult(input.ItemId, UpdateItemResultStatus.QueueNotFound);
        }

        return new UpdateItemResult(input.ItemId, UpdateItemResultStatus.Updated);
    }

    private async Task<UpdateItemResult> UpdateItemAndTryUpdateQueueToDoneAsync(UpdateItemExecutionInfoInput input)
    {
        //update item
        var updateItemResult = await _retryQueueItemRepository
            .UpdateItemAsync(input.ItemId, input.Status, input.AttemptCount, input.LastExecution, input.Description)
            .ConfigureAwait(false);

        if (updateItemResult.Status == UpdateItemResultStatus.ItemNotFound)
        {
            return updateItemResult;
        }

        // update queue last execution and try update queue to done
        var updateQueueResultStatus =
            await UpdateQueueLastExecutionAndTryUpdateQueueToDoneAsync(input.QueueId, input.LastExecution)
                .ConfigureAwait(false);

        if (updateQueueResultStatus == UpdateQueueResultStatus.QueueNotFound)
        {
            return new UpdateItemResult(input.ItemId, UpdateItemResultStatus.QueueNotFound);
        }

        return new UpdateItemResult(input.ItemId, UpdateItemResultStatus.Updated);
    }

    private async Task<UpdateQueueResult> UpdateQueueAndAllItemsAsync(UpdateItemsInQueueInput input)
    {
        var queue = await _retryQueueRepository.GetQueueAsync(input.QueueGroupKey).ConfigureAwait(false);

        if (queue is null)
        {
            return new UpdateQueueResult(input.QueueGroupKey, UpdateQueueResultStatus.QueueNotFound,
                RetryQueueStatus.None);
        }

        if (input.ItemStatus != RetryQueueItemStatus.Cancelled)
        {
            return new UpdateQueueResult(input.QueueGroupKey, UpdateQueueResultStatus.UpdateIsNotAllowed, queue.Status);
        }

        if (queue.Status != RetryQueueStatus.Active)
        {
            return new UpdateQueueResult(input.QueueGroupKey, UpdateQueueResultStatus.QueueIsNotActive, queue.Status);
        }

        var items = await _retryQueueItemRepository
            .GetItemsAsync(new[] { queue.Id }, new[] { RetryQueueItemStatus.Waiting })
            .ConfigureAwait(false);

        if (!items.Any())
        {
            return new UpdateQueueResult(input.QueueGroupKey, UpdateQueueResultStatus.QueueHasNoItemsWaiting,
                queue.Status);
        }

        foreach (var item in items)
        {
            var updateItemResult = await UpdateItemStatusAsync(new UpdateItemStatusInput(item.Id, input.ItemStatus))
                .ConfigureAwait(false);

            if (updateItemResult.Status != UpdateItemResultStatus.Updated)
            {
                return new UpdateQueueResult(input.QueueGroupKey, UpdateQueueResultStatus.FailedToUpdateAllItems,
                    queue.Status);
            }
        }

        var updateQueueResultStatus = await TryUpdateQueueToDoneAsync(queue.Id).ConfigureAwait(false);

        if (updateQueueResultStatus == UpdateQueueResultStatus.QueueNotFound)
        {
            return new UpdateQueueResult(input.QueueGroupKey,
                UpdateQueueResultStatus.AllItemsUpdatedButFailedToUpdateQueue, queue.Status);
        }

        queue = await _retryQueueRepository.GetQueueAsync(input.QueueGroupKey).ConfigureAwait(false);

        return new UpdateQueueResult(input.QueueGroupKey, updateQueueResultStatus, queue.Status);
    }

    private async Task<UpdateQueueResultStatus> UpdateQueueLastExecutionAndTryUpdateQueueToDoneAsync(Guid queueId,
        DateTime lastExecution)
    {
        var anyItemStillActive = await _retryQueueItemRepository.AnyItemStillActiveAsync(queueId).ConfigureAwait(false);

        if (anyItemStillActive)
        {
            // update queue last execution only
            var updateQueueLastExecutionResult = await _retryQueueRepository
                .UpdateLastExecutionAsync(queueId, lastExecution).ConfigureAwait(false);

            if (updateQueueLastExecutionResult.IsAcknowledged && updateQueueLastExecutionResult.MatchedCount == 0)
            {
                return UpdateQueueResultStatus.QueueNotFound;
            }
        }
        else
        {
            // update queue last execution and the status to done
            var updateQueueResult = await _retryQueueRepository
                .UpdateStatusAndLastExecutionAsync(queueId, RetryQueueStatus.Done, lastExecution).ConfigureAwait(false);

            if (updateQueueResult.IsAcknowledged && updateQueueResult.MatchedCount == 0)
            {
                return UpdateQueueResultStatus.QueueNotFound;
            }
        }

        return UpdateQueueResultStatus.Updated;
    }
}