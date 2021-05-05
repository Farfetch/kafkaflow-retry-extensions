namespace KafkaFlow.Retry.SqlServer.Readers
{
    using System.Collections.Generic;
    using KafkaFlow.Retry.Durable.Repository.Model;
    using KafkaFlow.Retry.SqlServer.Model;

    internal interface IRetryQueueReader
    {
        ICollection<RetryQueue> Read(RetryQueuesDboWrapper dboWrapper);
    }
}
