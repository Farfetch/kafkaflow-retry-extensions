using Dawn;
using KafkaFlow.Retry.Durable.Repository.Model;
using KafkaFlow.Retry.Postgres.Model;

namespace KafkaFlow.Retry.Postgres.Readers.Adapters
{
    internal class RetryQueueItemMessageAdapter : IRetryQueueItemMessageAdapter
    {
        public RetryQueueItemMessage Adapt(RetryQueueItemMessageDbo retryQueueItemMessageDbo)
        {
            Guard.Argument(retryQueueItemMessageDbo, nameof(retryQueueItemMessageDbo)).NotNull();

            return new RetryQueueItemMessage(
                retryQueueItemMessageDbo.TopicName,
                retryQueueItemMessageDbo.Key,
                retryQueueItemMessageDbo.Value,
                retryQueueItemMessageDbo.Partition,
                retryQueueItemMessageDbo.Offset,
                retryQueueItemMessageDbo.UtcTimeStamp);
        }
    }
}
