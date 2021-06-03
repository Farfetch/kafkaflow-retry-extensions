namespace KafkaFlow.Retry.API.Adapters.UpdateItems
{
    using Dawn;
    using KafkaFlow.Retry.API.Adapters.Common;
    using KafkaFlow.Retry.API.Dtos;
    using KafkaFlow.Retry.Durable.Repository.Actions.Update;

    internal class UpdateItemsInputAdapter : IUpdateItemsInputAdapter
    {
        private readonly IRetryQueueItemStatusDtoAdapter retryQueueItemStatusDtoAdapter;

        public UpdateItemsInputAdapter()
        {
            this.retryQueueItemStatusDtoAdapter = new RetryQueueItemStatusDtoAdapter();
        }

        public UpdateItemsInput Adapt(UpdateItemsRequestDto requestDto)
        {
            Guard.Argument(requestDto, nameof(requestDto)).NotNull();

            return new UpdateItemsInput(
               requestDto.ItemIds,
               this.retryQueueItemStatusDtoAdapter.Adapt(requestDto.Status)
               );
        }
    }
}