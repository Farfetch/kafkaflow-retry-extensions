using System.Collections.Generic;
using KafkaFlow.Retry.Durable.Repository.Model;
using KafkaFlow.Retry.SqlServer.Model;

namespace KafkaFlow.Retry.SqlServer.Readers;

internal interface IRetryQueueReader
{
    ICollection<RetryQueue> Read(RetryQueuesDboWrapper dboWrapper);
}