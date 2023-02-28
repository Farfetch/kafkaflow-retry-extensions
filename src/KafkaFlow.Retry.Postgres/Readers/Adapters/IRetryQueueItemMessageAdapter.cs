namespace KafkaFlow.Retry.Postgres.Readers.Adapters
{
    using KafkaFlow.Retry.Durable.Repository.Model;
    using KafkaFlow.Retry.Postgres.Model;
    
    internal interface IRetryQueueItemMessageAdapter : IDboDomainAdapter<RetryQueueItemMessageDbo, RetryQueueItemMessage>
    {
    }
}
