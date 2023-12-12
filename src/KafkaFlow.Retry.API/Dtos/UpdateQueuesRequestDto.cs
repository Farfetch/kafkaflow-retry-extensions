using System.Collections.Generic;
using KafkaFlow.Retry.API.Dtos.Common;

namespace KafkaFlow.Retry.API.Dtos;

public class UpdateQueuesRequestDto
{
    public RetryQueueItemStatusDto ItemStatus { get; set; }

    public IEnumerable<string> QueueGroupKeys { get; set; }
}