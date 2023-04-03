namespace KafkaFlow.Retry.Postgres.Model.Factories
{
    using System.Collections.Generic;
    using KafkaFlow.Retry.Durable.Repository.Model;
    
    internal interface IRetryQueueItemMessageHeaderDboFactory
    {
        IEnumerable<RetryQueueItemMessageHeaderDbo> Create(IEnumerable<MessageHeader> headers, long retryQueueItemId);
    }
}
