using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dawn;
using KafkaFlow.Retry.Durable.Common;
using KafkaFlow.Retry.Durable.Repository.Actions.Read;
using KafkaFlow.Retry.Durable.Repository.Actions.Update;
using KafkaFlow.Retry.Durable.Repository.Model;
using KafkaFlow.Retry.MongoDb.Model;
using MongoDB.Driver;

namespace KafkaFlow.Retry.MongoDb.Repositories;

internal class RetryQueueItemRepository : IRetryQueueItemRepository
{
    private readonly DbContext _dbContext;

    public RetryQueueItemRepository(DbContext dbContext)
    {
        Guard.Argument(dbContext).NotNull();

        _dbContext = dbContext;
    }

    public async Task<bool> AnyItemStillActiveAsync(Guid retryQueueId)
    {
        var itemsFilterBuilder = _dbContext.RetryQueueItems.GetFilters();

        var itemsFilter = itemsFilterBuilder.Eq(i => i.RetryQueueId, retryQueueId)
                          & itemsFilterBuilder.Nin(i => i.Status,
                              new[] { RetryQueueItemStatus.Done, RetryQueueItemStatus.Cancelled });

        var itemsDbo = await _dbContext.RetryQueueItems.GetAsync(itemsFilter).ConfigureAwait(false);

        return itemsDbo.Any();
    }

    public async Task DeleteItemsAsync(IEnumerable<Guid> queueIds)
    {
        var itemsFilterBuilder = _dbContext.RetryQueueItems.GetFilters();

        var deleteFilter = itemsFilterBuilder.In(i => i.RetryQueueId, queueIds);

        await _dbContext.RetryQueueItems.DeleteManyAsync(deleteFilter).ConfigureAwait(false);
    }

    public async Task<RetryQueueItemDbo> GetItemAsync(Guid itemId)
    {
        var queueItemsFilterBuilder = _dbContext.RetryQueueItems.GetFilters();

        var queueItemsFilter = queueItemsFilterBuilder.Eq(q => q.Id, itemId);

        var items = await _dbContext.RetryQueueItems.GetAsync(queueItemsFilter).ConfigureAwait(false);

        return items.FirstOrDefault();
    }

    public async Task<IEnumerable<RetryQueueItemDbo>> GetItemsAsync(
        IEnumerable<Guid> queueIds,
        IEnumerable<RetryQueueItemStatus> statuses,
        IEnumerable<SeverityLevel> severities = null,
        int? top = null,
        StuckStatusFilter stuckStatusFilter = null)
    {
        var itemsFilterBuilder = _dbContext.RetryQueueItems.GetFilters();

        var itemsFilter = itemsFilterBuilder.In(i => i.RetryQueueId, queueIds);

        if (stuckStatusFilter is null)
        {
            itemsFilter &= itemsFilterBuilder.In(i => i.Status, statuses);
        }
        else
        {
            itemsFilter &= itemsFilterBuilder.Or(
                itemsFilterBuilder.In(i => i.Status, statuses),
                itemsFilterBuilder.Eq(i => i.Status, stuckStatusFilter.ItemStatus)
                & itemsFilterBuilder.Lt(i => i.ModifiedStatusDate,
                    DateTime.UtcNow.AddSeconds(-stuckStatusFilter.ExpirationInterval.TotalSeconds)));
        }

        if (severities is object && severities.Any())
        {
            itemsFilter &= itemsFilterBuilder.In(i => i.SeverityLevel, severities);
        }

        var options = new FindOptions<RetryQueueItemDbo>
        {
            Sort = _dbContext.RetryQueueItems.GetSortDefinition().Ascending(i => i.Sort),
            Limit = top
        };

        return await _dbContext.RetryQueueItems.GetAsync(itemsFilter, options).ConfigureAwait(false);
    }

    public async Task<bool> IsFirstWaitingInQueue(RetryQueueItemDbo item)
    {
        var sortedItems = await GetItemsAsync(
                new[] { item.RetryQueueId },
                new[] { RetryQueueItemStatus.Waiting })
            .ConfigureAwait(false);

        if (sortedItems.Any() && item.Id == sortedItems.First().Id)
        {
            return true;
        }

        return false;
    }

    public async Task<UpdateItemResult> UpdateItemAsync(
        Guid itemId, RetryQueueItemStatus status, int attemptsCount, DateTime? lastExecution, string description)
    {
        var filter = _dbContext.RetryQueueItems.GetFilters().Eq(i => i.Id, itemId);

        var update = _dbContext.RetryQueueItems.GetUpdateDefinition()
            .Set(i => i.Status, status)
            .Set(i => i.AttemptsCount, attemptsCount)
            .Set(i => i.LastExecution, lastExecution)
            .Set(i => i.ModifiedStatusDate, DateTime.UtcNow);

        if (!string.IsNullOrEmpty(description))
        {
            update = update
                .Set(i => i.Description, description);
        }

        var updateResult = await _dbContext.RetryQueueItems.UpdateOneAsync(filter, update).ConfigureAwait(false);

        if (updateResult.IsAcknowledged && updateResult.MatchedCount == 0)
        {
            return new UpdateItemResult(itemId, UpdateItemResultStatus.ItemNotFound);
        }

        return new UpdateItemResult(itemId, UpdateItemResultStatus.Updated);
    }
}