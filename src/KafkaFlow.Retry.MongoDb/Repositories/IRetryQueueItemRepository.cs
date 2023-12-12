using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KafkaFlow.Retry.Durable.Common;
using KafkaFlow.Retry.Durable.Repository.Actions.Read;
using KafkaFlow.Retry.Durable.Repository.Actions.Update;
using KafkaFlow.Retry.Durable.Repository.Model;
using KafkaFlow.Retry.MongoDb.Model;

namespace KafkaFlow.Retry.MongoDb.Repositories;

internal interface IRetryQueueItemRepository
{
    Task<bool> AnyItemStillActiveAsync(Guid retryQueueId);

    Task DeleteItemsAsync(IEnumerable<Guid> queueIds);

    Task<RetryQueueItemDbo> GetItemAsync(Guid itemId);

    Task<IEnumerable<RetryQueueItemDbo>> GetItemsAsync(
        IEnumerable<Guid> queueIds,
        IEnumerable<RetryQueueItemStatus> statuses,
        IEnumerable<SeverityLevel> severities = null,
        int? top = null,
        StuckStatusFilter stuckStatusFilter = null);

    Task<bool> IsFirstWaitingInQueue(RetryQueueItemDbo item);

    Task<UpdateItemResult> UpdateItemAsync(
        Guid itemId,
        RetryQueueItemStatus status,
        int attemptsCount,
        DateTime? lastExecution,
        string description);
}