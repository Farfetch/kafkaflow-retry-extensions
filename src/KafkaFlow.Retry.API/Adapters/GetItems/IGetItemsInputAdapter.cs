namespace KafkaFlow.Retry.API.Adapters.GetItems
{
    using KafkaFlow.Retry.API.Dtos;
    using KafkaFlow.Retry.Durable.Repository.Actions.Read;

    public interface IGetItemsInputAdapter
    {
        GetQueuesInput Adapt(GetItemsRequestDto requestDto);
    }
}