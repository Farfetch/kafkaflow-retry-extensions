using System;
using System.Collections.Generic;
using KafkaFlow.Retry.API.Dtos.Common;

namespace KafkaFlow.Retry.API.Dtos;

public class UpdateItemsRequestDto
{
    public IEnumerable<Guid> ItemIds { get; set; }

    public RetryQueueItemStatusDto Status { get; set; }
}