namespace KafkaFlow.Retry.MongoDb.Adapters.Interfaces
{
    using KafkaFlow.Retry.Durable.Repository.Model;
    using KafkaFlow.Retry.MongoDb.Model;

    public interface IMessageAdapter
    {
        RetryQueueItemMessage Adapt(RetryQueueItemMessageDbo messageDbo);

        RetryQueueItemMessageDbo Adapt(RetryQueueItemMessage message);
    }
}