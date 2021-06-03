namespace KafkaFlow.Retry.API.Dtos
{
    using System.Collections.Generic;
    using KafkaFlow.Retry.API.Dtos.Common;

    public class GetItemsResponseDto
    {
        public GetItemsResponseDto(IEnumerable<RetryQueueItemDto> queueItemDtos)
        {
            this.QueueItems = queueItemDtos;
        }

        public IEnumerable<RetryQueueItemDto> QueueItems { get; set; }
    }
}
