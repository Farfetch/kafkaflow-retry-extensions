using KafkaFlow.Retry.API.Dtos;
using KafkaFlow.Retry.Durable.Repository;

namespace KafkaFlow.Retry.API.Adapters.UpdateQueues;

public interface IUpdateQueuesInputAdapter
{
    UpdateQueuesInput Adapt(UpdateQueuesRequestDto dto);
}