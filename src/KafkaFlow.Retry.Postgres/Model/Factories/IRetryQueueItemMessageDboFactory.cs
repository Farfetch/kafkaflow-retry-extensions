using KafkaFlow.Retry.Durable.Repository.Model;

namespace KafkaFlow.Retry.Postgres.Model.Factories
{
    internal interface IRetryQueueItemMessageDboFactory
    {
        RetryQueueItemMessageDbo Create(RetryQueueItemMessage retryQueueItemMessage, long retryQueueItemId);
    }
}
