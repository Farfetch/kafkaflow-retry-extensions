namespace KafkaFlow.Retry.API.Dtos
{
    using System.Collections.Generic;
    using KafkaFlow.Retry.Durable.Common;
    using KafkaFlow.Retry.Durable.Repository.Model;

    public class GetItemsRequestDto
    {
        public IEnumerable<RetryQueueItemStatus> ItemsStatuses { get; set; }
        public IEnumerable<SeverityLevel> SeverityLevels { get; set; }
        public int TopItemsByQueue { get; set; }
        public int TopQueues { get; set; }
    }
}