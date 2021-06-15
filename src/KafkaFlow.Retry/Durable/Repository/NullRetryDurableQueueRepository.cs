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
    internal class NullRetryDurableQueueRepository : IRetryDurableQueueRepository
    {
        public readonly static IRetryDurableQueueRepository Instance = new NullRetryDurableQueueRepository();

        public Task<AddIfQueueExistsResult> AddIfQueueExistsAsync(IMessageContext context)
            => Task.FromResult(new AddIfQueueExistsResult(AddIfQueueExistsResultStatus.NoPendingMembers));

        public Task<QueueNewestItemsResult> CheckQueueNewestItemsAsync(QueueNewestItemsInput queueNewestItemsInput)
            => Task.FromResult(new QueueNewestItemsResult(QueueNewestItemsResultStatus.NoNewestItems));

        public Task<QueuePendingItemsResult> CheckQueuePendingItemsAsync(QueuePendingItemsInput queuePendingItemsInput)
            => Task.FromResult(new QueuePendingItemsResult(QueuePendingItemsResultStatus.NoPendingItems));

        public Task<IEnumerable<RetryQueue>> GetRetryQueuesAsync(GetQueuesInput getQueuesInput)
                    => Task.FromResult(Enumerable.Empty<RetryQueue>());

        public Task<SaveToQueueResult> SaveToQueueAsync(IMessageContext context, string description)
            => Task.FromResult(new SaveToQueueResult(SaveToQueueResultStatus.Created));

        public Task UpdateItemAsync(UpdateItemInput updateItemInput)
            => Task.CompletedTask;
    }
}