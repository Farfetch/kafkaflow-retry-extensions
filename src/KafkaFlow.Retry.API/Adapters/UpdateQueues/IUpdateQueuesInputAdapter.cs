namespace KafkaFlow.Retry.API.Adapters.UpdateQueues
{
    using KafkaFlow.Retry.API.Dtos;
    using KafkaFlow.Retry.Durable.Repository;

    public interface IUpdateQueuesInputAdapter
    {
        UpdateQueuesInput Adapt(UpdateQueuesRequestDto dto);
    }
}