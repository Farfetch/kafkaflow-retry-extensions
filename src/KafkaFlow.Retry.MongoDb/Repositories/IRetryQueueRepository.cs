using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KafkaFlow.Retry.Durable.Repository.Actions.Delete;
using KafkaFlow.Retry.Durable.Repository.Actions.Read;
using KafkaFlow.Retry.Durable.Repository.Model;
using KafkaFlow.Retry.MongoDb.Model;
using MongoDB.Driver;

namespace KafkaFlow.Retry.MongoDb.Repositories;

internal interface IRetryQueueRepository
{
    Task<long> CountQueuesAsync(string searchGroupKey, RetryQueueStatus status);

    Task<DeleteQueuesResult> DeleteQueuesAsync(IEnumerable<Guid> queueIds);

    Task<RetryQueueDbo> GetQueueAsync(string queueGroupKey);

    Task<IEnumerable<Guid>> GetQueuesToDeleteAsync(string searchGroupKey, RetryQueueStatus status, DateTime maxLastExecutionDateToBeKept, int maxRowsToDelete);

    Task<IEnumerable<RetryQueueDbo>> GetTopSortedQueuesAsync(RetryQueueStatus status, GetQueuesSortOption sortOption, string searchGroupKey, int top);

    Task<UpdateResult> UpdateLastExecutionAsync(Guid queueId, DateTime lastExecution);

    Task<UpdateResult> UpdateStatusAndLastExecutionAsync(Guid queueId, RetryQueueStatus status, DateTime lastExecution);

    Task<UpdateResult> UpdateStatusAsync(Guid queueId, RetryQueueStatus status);
}