using System.Collections.Generic;
using System.Threading.Tasks;
using KafkaFlow.Retry.Durable.Repository.Actions.Create;
using KafkaFlow.Retry.Durable.Repository.Actions.Delete;
using KafkaFlow.Retry.Durable.Repository.Actions.Read;
using KafkaFlow.Retry.Durable.Repository.Actions.Update;
using KafkaFlow.Retry.Durable.Repository.Model;

namespace KafkaFlow.Retry.Durable.Repository;

internal interface IRetryDurableQueueRepository
{
    Task<AddIfQueueExistsResult> AddIfQueueExistsAsync(IMessageContext context);

    Task<QueueNewestItemsResult> CheckQueueNewestItemsAsync(QueueNewestItemsInput queueNewestItemsInput);

    Task<QueuePendingItemsResult> CheckQueuePendingItemsAsync(QueuePendingItemsInput queuePendingItemsInput);

    Task<DeleteQueuesResult> DeleteQueuesAsync(DeleteQueuesInput deleteQueuesInput);

    Task<IEnumerable<RetryQueue>> GetRetryQueuesAsync(GetQueuesInput getQueuesInput);

    Task<SaveToQueueResult> SaveToQueueAsync(IMessageContext context, string description);

    Task UpdateItemAsync(UpdateItemInput updateItemInput);
}