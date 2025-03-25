using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dawn;
using KafkaFlow.Retry.Durable.Repository.Actions.Delete;
using KafkaFlow.Retry.Durable.Repository.Actions.Read;
using KafkaFlow.Retry.Durable.Repository.Model;
using KafkaFlow.Retry.MongoDb.Model;
using MongoDB.Driver;

namespace KafkaFlow.Retry.MongoDb.Repositories;

internal class RetryQueueRepository : IRetryQueueRepository
{
    private readonly DbContext _dbContext;

    public RetryQueueRepository(DbContext dbContext)
    {
        Guard.Argument(dbContext).NotNull();

        _dbContext = dbContext;
    }

    public async Task<long> CountQueuesAsync(string searchGroupKey, RetryQueueStatus status)
    {
        var queuesFilterBuilder = _dbContext.RetryQueues.GetFilters();

        var findFilter = queuesFilterBuilder.Eq(q => q.SearchGroupKey, searchGroupKey)
                         & queuesFilterBuilder.Eq(q => q.Status, status);

        return await _dbContext.RetryQueues.CountDocumentsAsync(findFilter).ConfigureAwait(false);
    }

    public async Task<DeleteQueuesResult> DeleteQueuesAsync(IEnumerable<Guid> queueIds)
    {
        var queuesFilterBuilder = _dbContext.RetryQueues.GetFilters();

        var deleteFilter = queuesFilterBuilder.In(q => q.Id, queueIds);

        var deleteResult = await _dbContext.RetryQueues.DeleteManyAsync(deleteFilter).ConfigureAwait(false);

        return new DeleteQueuesResult(GetDeletedCount(deleteResult));
    }

    public async Task<RetryQueueDbo> GetQueueAsync(string queueGroupKey)
    {
        var queuesFilterBuilder = _dbContext.RetryQueues.GetFilters();

        var queuesFilter = queuesFilterBuilder.Eq(q => q.QueueGroupKey, queueGroupKey);

        return await _dbContext.RetryQueues.GetOneAsync(queuesFilter).ConfigureAwait(false);
    }

    public async Task<IEnumerable<Guid>> GetQueuesToDeleteAsync(string searchGroupKey, RetryQueueStatus status,
        DateTime maxLastExecutionDateToBeKept, int maxRowsToDelete)
    {
        var queuesFilterBuilder = _dbContext.RetryQueues.GetFilters();

        var findFilter = queuesFilterBuilder.Eq(q => q.SearchGroupKey, searchGroupKey)
                         & queuesFilterBuilder.Eq(q => q.Status, status)
                         & queuesFilterBuilder.Lt(q => q.LastExecution, maxLastExecutionDateToBeKept);

        var options = new FindOptions<RetryQueueDbo>
        {
            Limit = maxRowsToDelete
        };

        var queuesToDelete = await _dbContext.RetryQueues.GetAsync(findFilter, options).ConfigureAwait(false);

        return queuesToDelete.Select(q => q.Id);
    }

    public async Task<IEnumerable<RetryQueueDbo>> GetTopSortedQueuesAsync(RetryQueueStatus status,
        GetQueuesSortOption sortOption, string searchGroupKey, int top)
    {
        var queuesFilterBuilder = _dbContext.RetryQueues.GetFilters();

        var queuesFilter = queuesFilterBuilder.Eq(q => q.Status, status);

        if (searchGroupKey is object)
        {
            queuesFilter &= queuesFilterBuilder.Eq(q => q.SearchGroupKey, searchGroupKey);
        }

        SortDefinition<RetryQueueDbo> sortDefinition;

        switch (sortOption)
        {
            case GetQueuesSortOption.ByLastExecutionAscending:
                sortDefinition = _dbContext.RetryQueues.GetSortDefinition().Ascending(i => i.LastExecution);
                break;

            case GetQueuesSortOption.ByCreationDateDescending:
            default:
                sortDefinition = _dbContext.RetryQueues.GetSortDefinition().Descending(i => i.CreationDate);
                break;
        }

        var options = new FindOptions<RetryQueueDbo>
        {
            Sort = sortDefinition,
            Limit = top
        };

        return await _dbContext.RetryQueues.GetAsync(queuesFilter, options).ConfigureAwait(false);
    }

    public async Task<UpdateResult> UpdateLastExecutionAsync(Guid queueId, DateTime lastExecution)
    {
        var filter = _dbContext.RetryQueues.GetFilters().Eq(q => q.Id, queueId);

        var update = _dbContext.RetryQueues.GetUpdateDefinition()
            .Set(q => q.LastExecution, lastExecution);

        return await _dbContext.RetryQueues.UpdateOneAsync(filter, update).ConfigureAwait(false);
    }

    public async Task<UpdateResult> UpdateStatusAndLastExecutionAsync(Guid queueId, RetryQueueStatus status,
        DateTime lastExecution)
    {
        var filter = _dbContext.RetryQueues.GetFilters().Eq(q => q.Id, queueId);

        var update = _dbContext.RetryQueues.GetUpdateDefinition()
            .Set(q => q.Status, status)
            .Set(q => q.LastExecution, lastExecution);

        return await _dbContext.RetryQueues.UpdateOneAsync(filter, update).ConfigureAwait(false);
    }

    public async Task<UpdateResult> UpdateStatusAsync(Guid queueId, RetryQueueStatus status)
    {
        var filter = _dbContext.RetryQueues.GetFilters().Eq(q => q.Id, queueId);

        var update = _dbContext.RetryQueues.GetUpdateDefinition()
            .Set(q => q.Status, status);

        return await _dbContext.RetryQueues.UpdateOneAsync(filter, update).ConfigureAwait(false);
    }

    private int GetDeletedCount(DeleteResult deleteResult)
    {
        return deleteResult.IsAcknowledged ? Convert.ToInt32(deleteResult.DeletedCount) : 0;
    }
}