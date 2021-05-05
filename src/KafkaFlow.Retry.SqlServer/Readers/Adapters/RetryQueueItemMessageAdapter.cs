namespace KafkaFlow.Retry.SqlServer.Readers.Adapters
{
    using Dawn;
    using KafkaFlow.Retry.Durable.Repository.Model;
    using KafkaFlow.Retry.SqlServer.Model;

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
