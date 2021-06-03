namespace KafkaFlow.Retry.API.Dtos
{
    using System;
    using System.Collections.Generic;
    using KafkaFlow.Retry.API.Dtos.Common;

    public class UpdateItemsRequestDto
    {
        public IEnumerable<Guid> ItemIds { get; set; }

        public RetryQueueItemStatusDto Status { get; set; }
    }
}
