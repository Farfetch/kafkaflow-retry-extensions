using Dawn;
using KafkaFlow.Retry.Durable.Repository.Model;
using KafkaFlow.Retry.Postgres.Model;

namespace KafkaFlow.Retry.Postgres.Readers.Adapters;

internal class RetryQueueItemAdapter : IRetryQueueItemAdapter
{
    public RetryQueueItem Adapt(RetryQueueItemDbo retryQueueItemDbo)
    {
        Guard.Argument(retryQueueItemDbo).NotNull();

        return new RetryQueueItem(
            retryQueueItemDbo.IdDomain,
            retryQueueItemDbo.AttemptsCount,
            retryQueueItemDbo.CreationDate,
            retryQueueItemDbo.Sort,
            retryQueueItemDbo.LastExecution,
            retryQueueItemDbo.ModifiedStatusDate,
            retryQueueItemDbo.Status,
            retryQueueItemDbo.SeverityLevel,
            retryQueueItemDbo.Description);
    }
}