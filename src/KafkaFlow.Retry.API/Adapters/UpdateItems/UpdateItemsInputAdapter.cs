using Dawn;
using KafkaFlow.Retry.API.Adapters.Common;
using KafkaFlow.Retry.API.Dtos;
using KafkaFlow.Retry.Durable.Repository.Actions.Update;

namespace KafkaFlow.Retry.API.Adapters.UpdateItems;

internal class UpdateItemsInputAdapter : IUpdateItemsInputAdapter
{
    private readonly IRetryQueueItemStatusDtoAdapter _retryQueueItemStatusDtoAdapter;

    public UpdateItemsInputAdapter()
    {
        _retryQueueItemStatusDtoAdapter = new RetryQueueItemStatusDtoAdapter();
    }

    public UpdateItemsInput Adapt(UpdateItemsRequestDto requestDto)
    {
        Guard.Argument(requestDto, nameof(requestDto)).NotNull();

        return new UpdateItemsInput(
            requestDto.ItemIds,
            _retryQueueItemStatusDtoAdapter.Adapt(requestDto.Status)
        );
    }
}