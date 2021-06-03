namespace KafkaFlow.Retry.API.Adapters.GetItems
{
    using System.Collections.Generic;
    using Dawn;
    using KafkaFlow.Retry.API.Adapters.Common;
    using KafkaFlow.Retry.API.Dtos;
    using KafkaFlow.Retry.API.Dtos.Common;
    using KafkaFlow.Retry.Durable.Repository.Actions.Read;

    internal class GetItemsResponseDtoAdapter : IGetItemsResponseDtoAdapter
    {
        private readonly IRetryQueueItemAdapter retryQueueItemAdapter;

        public GetItemsResponseDtoAdapter()
        {
            this.retryQueueItemAdapter = new RetryQueueItemAdapter();
        }

        public GetItemsResponseDto Adapt(GetQueuesResult getQueuesResult)
        {
            Guard.Argument(getQueuesResult, nameof(getQueuesResult)).NotNull();
            Guard.Argument(getQueuesResult.RetryQueues, nameof(getQueuesResult.RetryQueues)).NotNull();

            var itemsDto = new List<RetryQueueItemDto>();

            foreach (var queue in getQueuesResult.RetryQueues)
            {
                foreach (var item in queue.Items)
                {
                    itemsDto.Add(this.retryQueueItemAdapter.Adapt(item, queue.QueueGroupKey));
                }
            }

            return new GetItemsResponseDto(itemsDto);
        }
    }
}