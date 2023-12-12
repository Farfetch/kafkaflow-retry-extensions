using System.Collections.Generic;
using KafkaFlow.Retry.Durable.Repository.Model;
using KafkaFlow.Retry.MongoDb.Model;

namespace KafkaFlow.Retry.MongoDb.Adapters.Interfaces;

internal interface IQueuesAdapter
{
    IEnumerable<RetryQueue> Adapt(IEnumerable<RetryQueueDbo> queuesDbo, IEnumerable<RetryQueueItemDbo> itemsDbo);
}