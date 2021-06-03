namespace KafkaFlow.Retry.API.Adapters.UpdateQueues
{
    using Dawn;
    using KafkaFlow.Retry.API.Dtos;
    using KafkaFlow.Retry.Durable.Repository.Actions.Update;

    internal class UpdateQueuesResponseDtoAdapter : IUpdateQueuesResponseDtoAdapter
    {
        public UpdateQueuesResponseDto Adapt(UpdateQueuesResult updateQueuesResult)
        {
            Guard.Argument(updateQueuesResult, nameof(updateQueuesResult)).NotNull();

            var resultDto = new UpdateQueuesResponseDto();

            foreach (var res in updateQueuesResult.Results)
            {
                resultDto.UpdateQueuesResults.Add(new UpdateQueueResultDto(res.QueueGroupKey, res.Status, res.RetryQueueStatus));
            }

            return resultDto;
        }
    }
}