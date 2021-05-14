namespace KafkaFlow.Retry.Durable.Repository
{
    using System.Threading.Tasks;
    using KafkaFlow.Retry.Durable.Repository.Actions.Create;
    using KafkaFlow.Retry.Durable.Repository.Actions.Read;
    using KafkaFlow.Retry.Durable.Repository.Actions.Update;

    public interface IKafkaRetryDurableQueueRepositoryProvider
    {
        Task<CheckQueueResult> CheckQueueAsync(CheckQueueInput input);

        Task<QueuePendingItemsResult> CheckQueuePendingItemsAsync(QueuePendingItemsInput input);

        Task<GetQueuesResult> GetQueuesAsync(GetQueuesInput input);

        Task<SaveToQueueResult> SaveToQueueAsync(SaveToQueueInput input);

        Task<UpdateItemResult> UpdateItemExecutionInfoAsync(UpdateItemExecutionInfoInput input);

        Task<UpdateItemsResult> UpdateItemsAsync(UpdateItemsInput input);

        Task<UpdateItemResult> UpdateItemStatusAsync(UpdateItemStatusInput input);

        Task<UpdateQueuesResult> UpdateQueuesAsync(UpdateQueuesInput input);
    }
}