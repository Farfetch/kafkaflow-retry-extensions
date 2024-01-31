using KafkaFlow.Retry.API.Dtos;
using KafkaFlow.Retry.Durable.Repository.Actions.Update;

namespace KafkaFlow.Retry.API.Adapters.UpdateItems;

public interface IUpdateItemsInputAdapter
{
    UpdateItemsInput Adapt(UpdateItemsRequestDto requestDto);
}