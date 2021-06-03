namespace KafkaFlow.Retry.API.Adapters.UpdateItems
{
    using KafkaFlow.Retry.API.Dtos;
    using KafkaFlow.Retry.Durable.Repository.Actions.Update;

    public interface IUpdateItemsResponseDtoAdapter
    {
        UpdateItemsResponseDto Adapt(UpdateItemsResult updateItemsResult);
    }
}