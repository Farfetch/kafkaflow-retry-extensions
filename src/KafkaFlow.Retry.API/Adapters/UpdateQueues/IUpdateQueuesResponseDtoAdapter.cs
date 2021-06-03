namespace KafkaFlow.Retry.API.Adapters.UpdateQueues
{
    using KafkaFlow.Retry.API.Dtos;
    using KafkaFlow.Retry.Durable.Repository.Actions.Update;

    public interface IUpdateQueuesResponseDtoAdapter
    {
        UpdateQueuesResponseDto Adapt(UpdateQueuesResult updateQueuesResult);
    }
}