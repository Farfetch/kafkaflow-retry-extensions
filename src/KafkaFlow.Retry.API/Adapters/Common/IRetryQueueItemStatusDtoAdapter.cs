namespace KafkaFlow.Retry.API.Adapters.Common
{
    using KafkaFlow.Retry.API.Dtos.Common;
    using KafkaFlow.Retry.Durable.Repository.Model;

    internal interface IRetryQueueItemStatusDtoAdapter
    {
        RetryQueueItemStatus Adapt(RetryQueueItemStatusDto dto);
    }
}