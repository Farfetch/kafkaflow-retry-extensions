using KafkaFlow.Retry.API.Dtos;
using KafkaFlow.Retry.Durable.Repository.Actions.Read;

namespace KafkaFlow.Retry.API.Adapters.GetItems;

public interface IGetItemsInputAdapter
{
    GetQueuesInput Adapt(GetItemsRequestDto requestDto);
}