using KafkaFlow.Retry.Durable.Repository.Model;
using KafkaFlow.Retry.SqlServer.Model;

namespace KafkaFlow.Retry.SqlServer.Readers.Adapters;

internal interface IRetryQueueItemAdapter : IDboDomainAdapter<RetryQueueItemDbo, RetryQueueItem>
{
}