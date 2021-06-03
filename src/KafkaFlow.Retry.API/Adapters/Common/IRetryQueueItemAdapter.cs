namespace KafkaFlow.Retry.API.Adapters.Common
{
    using KafkaFlow.Retry.API.Dtos.Common;
    using KafkaFlow.Retry.Durable.Repository.Model;

    internal interface IRetryQueueItemAdapter
    {
        RetryQueueItemDto Adapt(RetryQueueItem item, string queueGroupKey);
    }
}