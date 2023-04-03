namespace KafkaFlow.Retry.Postgres.Model.Factories
{
    using System;
    using KafkaFlow.Retry.Durable.Repository.Actions.Create;
    
    internal interface IRetryQueueItemDboFactory
    {
        RetryQueueItemDbo Create(SaveToQueueInput input, long retryQueueId, Guid retryQueueDomainId);
    }
}