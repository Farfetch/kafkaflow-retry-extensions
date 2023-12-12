using KafkaFlow.Retry.API.Dtos.Common;
using KafkaFlow.Retry.Durable.Repository.Model;

namespace KafkaFlow.Retry.API.Adapters.Common;

internal class RetryQueueItemStatusDtoAdapter : IRetryQueueItemStatusDtoAdapter
{
    public RetryQueueItemStatus Adapt(RetryQueueItemStatusDto dto)
    {
            switch (dto)
            {
                case RetryQueueItemStatusDto.Waiting:
                    return RetryQueueItemStatus.Waiting;

                case RetryQueueItemStatusDto.Done:
                    return RetryQueueItemStatus.Done;

                case RetryQueueItemStatusDto.InRetry:
                    return RetryQueueItemStatus.InRetry;

                case RetryQueueItemStatusDto.Cancelled:
                    return RetryQueueItemStatus.Cancelled;

                default:
                    return RetryQueueItemStatus.None;
            }
        }
}