namespace KafkaFlow.Retry.MongoDb.Adapters.Interfaces
{
    using KafkaFlow.Retry.Durable.Repository.Model;
    using KafkaFlow.Retry.MongoDb.Model;

    public interface IHeaderAdapter
    {
        RetryQueueHeaderDbo Adapt(MessageHeader header);

        MessageHeader Adapt(RetryQueueHeaderDbo headerDbo);
    }
}