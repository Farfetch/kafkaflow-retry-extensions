using System.Collections.Generic;
using KafkaFlow.Retry.API.Dtos.Common;

namespace KafkaFlow.Retry.API.Dtos;
public class GetItemsResponseDto
{
    public GetItemsResponseDto(IEnumerable<RetryQueueItemDto> queueItemDtos)
    {
            QueueItems = queueItemDtos;
        }

    public IEnumerable<RetryQueueItemDto> QueueItems { get; set; }
}