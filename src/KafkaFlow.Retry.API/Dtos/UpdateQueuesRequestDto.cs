namespace KafkaFlow.Retry.API.Dtos
{
    using System.Collections.Generic;
    using KafkaFlow.Retry.API.Dtos.Common;

    public class UpdateQueuesRequestDto
    {
        public RetryQueueItemStatusDto ItemStatus { get; set; }

        public IEnumerable<string> QueueGroupKeys { get; set; }
    }
}
