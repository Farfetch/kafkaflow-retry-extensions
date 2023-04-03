namespace KafkaFlow.Retry.Postgres.Model.Factories
{
    using Dawn;
    using KafkaFlow.Retry.Durable.Repository.Model;
    
    internal sealed class RetryQueueItemMessageDboFactory : IRetryQueueItemMessageDboFactory
    {
        public RetryQueueItemMessageDbo Create(RetryQueueItemMessage retryQueueItemMessage, long retryQueueItemId)
        {
            Guard.Argument(retryQueueItemMessage, nameof(retryQueueItemMessage)).NotNull();
            Guard.Argument(retryQueueItemId, nameof(retryQueueItemId)).Positive();

            return new RetryQueueItemMessageDbo
            {
                IdRetryQueueItem = retryQueueItemId,
                Key = retryQueueItemMessage.Key,
                Value = retryQueueItemMessage.Value,
                Offset = retryQueueItemMessage.Offset,
                Partition = retryQueueItemMessage.Partition,
                TopicName = retryQueueItemMessage.TopicName,
                UtcTimeStamp = retryQueueItemMessage.UtcTimeStamp,
            };
        }
    }
}
