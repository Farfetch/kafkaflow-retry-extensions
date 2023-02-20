using KafkaFlow.Retry.Durable.Repository.Model;
using KafkaFlow.Retry.Postgres.Model;

namespace KafkaFlow.Retry.Postgres.Readers.Adapters
{
    internal interface IRetryQueueItemAdapter : IDboDomainAdapter<RetryQueueItemDbo, RetryQueueItem>
    {
    }
}
