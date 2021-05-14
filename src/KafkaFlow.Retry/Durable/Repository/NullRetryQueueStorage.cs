namespace KafkaFlow.Retry.Durable.Repository
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading.Tasks;
    using KafkaFlow.Retry.Durable.Repository.Actions.Create;
    using KafkaFlow.Retry.Durable.Repository.Actions.Read;
    using KafkaFlow.Retry.Durable.Repository.Actions.Update;
    using KafkaFlow.Retry.Durable.Repository.Model;

    [ExcludeFromCodeCoverage]
    internal class NullRetryQueueStorage : IKafkaRetryDurableQueueRepository
    {
        public static IKafkaRetryDurableQueueRepository Instance = new NullRetryQueueStorage();

        public Task<AddIfQueueExistsResult> AddIfQueueExistsAsync(IMessageContext context)
            => Task.FromResult(new AddIfQueueExistsResult(AddIfQueueExistsResultStatus.NoPendingMembers));

        public Task<QueuePendingItemsResult> CheckQueuePendingItemsAsync(QueuePendingItemsInput input)
            => Task.FromResult(new QueuePendingItemsResult(QueuePendingItemsResultStatus.NoPendingItems));

        public Task<IEnumerable<RetryQueue>> GetRetryQueuesAsync(GetQueuesInput input)
                    => Task.FromResult(Enumerable.Empty<RetryQueue>());

        public Task<SaveToQueueResult> SaveToQueueAsync(IMessageContext context, string description)
            => Task.FromResult(new SaveToQueueResult(SaveToQueueResultStatus.Created));

        public Task UpdateItemAsync(UpdateItemInput input)
            => Task.CompletedTask;
    }
}