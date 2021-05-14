namespace KafkaFlow.Retry.Durable.Repository
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using KafkaFlow.Retry.Durable.Repository.Actions.Create;
    using KafkaFlow.Retry.Durable.Repository.Actions.Read;
    using KafkaFlow.Retry.Durable.Repository.Actions.Update;
    using KafkaFlow.Retry.Durable.Repository.Model;

    internal interface IKafkaRetryDurableQueueRepository
    {
        Task<AddIfQueueExistsResult> AddIfQueueExistsAsync(IMessageContext context);

        Task<QueuePendingItemsResult> CheckQueuePendingItemsAsync(QueuePendingItemsInput input);

        Task<IEnumerable<RetryQueue>> GetRetryQueuesAsync(GetQueuesInput input);

        Task<SaveToQueueResult> SaveToQueueAsync(IMessageContext context, string description);

        Task UpdateItemAsync(UpdateItemInput input);
    }
}