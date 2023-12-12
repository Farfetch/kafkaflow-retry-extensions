using Dawn;
using KafkaFlow.Retry.API.Adapters.Common;
using KafkaFlow.Retry.API.Dtos;
using KafkaFlow.Retry.Durable.Repository;

namespace KafkaFlow.Retry.API.Adapters.UpdateQueues;

internal class UpdateQueuesInputAdapter : IUpdateQueuesInputAdapter
{
    private readonly IRetryQueueItemStatusDtoAdapter retryQueueItemStatusDtoAdapter;

    public UpdateQueuesInputAdapter()
    {
        this.retryQueueItemStatusDtoAdapter = new RetryQueueItemStatusDtoAdapter();
    }

    public UpdateQueuesInput Adapt(UpdateQueuesRequestDto requestDto)
    {
        Guard.Argument(requestDto, nameof(requestDto)).NotNull();

        return new UpdateQueuesInput(
            requestDto.QueueGroupKeys,
            this.retryQueueItemStatusDtoAdapter.Adapt(requestDto.ItemStatus)
        );
    }
}