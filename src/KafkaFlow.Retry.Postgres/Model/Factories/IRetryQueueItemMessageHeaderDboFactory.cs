using System.Collections.Generic;
using KafkaFlow.Retry.Durable.Repository.Model;

namespace KafkaFlow.Retry.Postgres.Model.Factories
{
    internal interface IRetryQueueItemMessageHeaderDboFactory
    {
        IEnumerable<RetryQueueItemMessageHeaderDbo> Create(IEnumerable<MessageHeader> headers, long retryQueueItemId);
    }
}
