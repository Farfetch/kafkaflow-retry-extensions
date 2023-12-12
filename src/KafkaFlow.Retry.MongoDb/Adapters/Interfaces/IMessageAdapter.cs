using KafkaFlow.Retry.Durable.Repository.Model;
using KafkaFlow.Retry.MongoDb.Model;

namespace KafkaFlow.Retry.MongoDb.Adapters.Interfaces;

public interface IMessageAdapter
{
    RetryQueueItemMessage Adapt(RetryQueueItemMessageDbo messageDbo);

    RetryQueueItemMessageDbo Adapt(RetryQueueItemMessage message);
}