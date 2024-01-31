using System;
using KafkaFlow.Retry.Durable.Repository.Actions.Create;

namespace KafkaFlow.Retry.Postgres.Model.Factories;

internal interface IRetryQueueItemDboFactory
{
    RetryQueueItemDbo Create(SaveToQueueInput input, long retryQueueId, Guid retryQueueDomainId);
}