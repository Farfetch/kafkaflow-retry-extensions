using Dawn;
using KafkaFlow.Retry.Durable.Repository.Model;

namespace KafkaFlow.Retry.SqlServer.Model.Factories;

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
            UtcTimeStamp = retryQueueItemMessage.UtcTimeStamp
        };
    }
}