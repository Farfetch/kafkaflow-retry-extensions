namespace KafkaFlow.Retry.IntegrationTests.Core.Storages.Models
{
    using System;

    public class RetryQueueItemTestModel
    {
        public int AttemptsCount { get; set; }

        public DateTime CreationDate { get; set; }

        public string Description { get; set; }

        public Guid Id { get; set; }

        public DateTime? LastExecution { get; set; }

        public RetryQueueItemMessageTestModel Message { get; set; }

        public DateTime? ModifiedStatusDate { get; set; }

        public Guid RetryQueueId { get; set; }

        public int Sort { get; set; }

        public RetryQueueItemStatusTestModel Status { get; set; }
    }
}