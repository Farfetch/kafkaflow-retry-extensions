using KafkaFlow.Retry.API.Dtos;
using KafkaFlow.Retry.Durable.Repository.Actions.Update;

namespace KafkaFlow.Retry.API.Adapters.UpdateQueues;

public interface IUpdateQueuesResponseDtoAdapter
{
    UpdateQueuesResponseDto Adapt(UpdateQueuesResult updateQueuesResult);
}