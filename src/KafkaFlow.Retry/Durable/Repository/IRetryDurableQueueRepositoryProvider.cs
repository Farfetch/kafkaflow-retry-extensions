using System.Threading.Tasks;
using KafkaFlow.Retry.Durable.Repository.Actions.Create;
using KafkaFlow.Retry.Durable.Repository.Actions.Delete;
using KafkaFlow.Retry.Durable.Repository.Actions.Read;
using KafkaFlow.Retry.Durable.Repository.Actions.Update;

namespace KafkaFlow.Retry.Durable.Repository;

public interface IRetryDurableQueueRepositoryProvider
{
    Task<CheckQueueResult> CheckQueueAsync(CheckQueueInput input);

    Task<QueueNewestItemsResult> CheckQueueNewestItemsAsync(QueueNewestItemsInput input);

    Task<QueuePendingItemsResult> CheckQueuePendingItemsAsync(QueuePendingItemsInput input);

    Task<DeleteQueuesResult> DeleteQueuesAsync(DeleteQueuesInput input);

    Task<GetQueuesResult> GetQueuesAsync(GetQueuesInput input);

    Task<long> CountQueuesAsync(CountQueuesInput input);

    Task<SaveToQueueResult> SaveToQueueAsync(SaveToQueueInput input);

    Task<UpdateItemResult> UpdateItemExecutionInfoAsync(UpdateItemExecutionInfoInput input);

    Task<UpdateItemsResult> UpdateItemsAsync(UpdateItemsInput input);

    Task<UpdateItemResult> UpdateItemStatusAsync(UpdateItemStatusInput input);

    Task<UpdateQueuesResult> UpdateQueuesAsync(UpdateQueuesInput input);
}