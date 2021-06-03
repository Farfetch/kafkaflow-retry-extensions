namespace KafkaFlow.Retry.API.Dtos.Common
{
    using System;
    using KafkaFlow.Retry.Durable.Common;
    using KafkaFlow.Retry.Durable.Repository.Model;

    public class RetryQueueItemDto
    {
        public int AttemptsCount { get; set; }

        public DateTime CreationDate { get; set; }

        public string Description { get; set; }

        public Guid Id { get; set; }

        public DateTime? LastExecution { get; set; }

        public RetryQueuetItemMessageInfoDto MessageInfo { get; set; }

        public string QueueGroupKey { get; set; }

        public SeverityLevel SeverityLevel { get; set; }

        public int Sort { get; set; }

        public RetryQueueItemStatus Status { get; set; }
    }
}