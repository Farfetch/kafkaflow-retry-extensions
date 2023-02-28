namespace KafkaFlow.Retry.Postgres.Readers
{
    using System.Collections.Generic;
    using KafkaFlow.Retry.Durable.Repository.Model;
    using KafkaFlow.Retry.Postgres.Model;
    
    internal interface IRetryQueueReader
    {
        ICollection<RetryQueue> Read(RetryQueuesDboWrapper dboWrapper);
    }
}
