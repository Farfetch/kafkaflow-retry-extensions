using System.Collections.Generic;
using KafkaFlow.Retry.Durable.Repository.Model;
using KafkaFlow.Retry.Postgres.Model;

namespace KafkaFlow.Retry.Postgres.Readers;

internal interface IRetryQueueReader
{
    ICollection<RetryQueue> Read(RetryQueuesDboWrapper dboWrapper);
}