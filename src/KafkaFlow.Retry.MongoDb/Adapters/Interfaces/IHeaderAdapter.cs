using KafkaFlow.Retry.Durable.Repository.Model;
using KafkaFlow.Retry.MongoDb.Model;

namespace KafkaFlow.Retry.MongoDb.Adapters.Interfaces;

public interface IHeaderAdapter
{
    RetryQueueHeaderDbo Adapt(MessageHeader header);

    MessageHeader Adapt(RetryQueueHeaderDbo headerDbo);
}