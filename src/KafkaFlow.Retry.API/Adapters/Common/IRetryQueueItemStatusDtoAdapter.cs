using KafkaFlow.Retry.API.Dtos.Common;
using KafkaFlow.Retry.Durable.Repository.Model;

namespace KafkaFlow.Retry.API.Adapters.Common;

internal interface IRetryQueueItemStatusDtoAdapter
{
    RetryQueueItemStatus Adapt(RetryQueueItemStatusDto dto);
}