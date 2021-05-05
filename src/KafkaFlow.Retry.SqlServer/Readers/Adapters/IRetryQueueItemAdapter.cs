namespace KafkaFlow.Retry.SqlServer.Readers.Adapters
{
    using KafkaFlow.Retry.Durable.Repository.Model;
    using KafkaFlow.Retry.SqlServer.Model;

    internal interface IRetryQueueItemAdapter : IDboDomainAdapter<RetryQueueItemDbo, RetryQueueItem>
    {
    }
}
