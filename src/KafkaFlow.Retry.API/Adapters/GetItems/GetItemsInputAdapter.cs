using Dawn;
using KafkaFlow.Retry.API.Dtos;
using KafkaFlow.Retry.Durable.Repository.Actions.Read;
using KafkaFlow.Retry.Durable.Repository.Model;

namespace KafkaFlow.Retry.API.Adapters.GetItems;

internal class GetItemsInputAdapter : IGetItemsInputAdapter
{
    private readonly GetQueuesSortOption sortOption = GetQueuesSortOption.ByCreationDate_Descending;

    public GetQueuesInput Adapt(GetItemsRequestDto requestDto)
    {
        Guard.Argument(requestDto, nameof(requestDto)).NotNull();

        return new GetQueuesInput(RetryQueueStatus.Active, requestDto.ItemsStatuses, this.sortOption, requestDto.TopQueues)
        {
            SeverityLevels = requestDto.SeverityLevels,
            TopItemsByQueue = requestDto.TopItemsByQueue
        };
    }
}