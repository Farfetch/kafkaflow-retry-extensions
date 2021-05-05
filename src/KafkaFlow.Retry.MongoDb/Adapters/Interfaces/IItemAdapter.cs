namespace KafkaFlow.Retry.MongoDb.Adapters.Interfaces
{
    using KafkaFlow.Retry.Durable.Repository.Model;
    using KafkaFlow.Retry.MongoDb.Model;

    public interface IItemAdapter
    {
        RetryQueueItem Adapt(RetryQueueItemDbo itemDbo);
    }
}