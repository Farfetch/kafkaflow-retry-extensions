namespace KafkaFlow.Retry.IntegrationTests.Core.Storages.Models
{
    using System;

    public class RetryQueueTestModel
    {
        public DateTime CreationDate { get; set; }

        public Guid Id { get; set; }

        public DateTime LastExecution { get; set; }

        public string QueueGroupKey { get; set; }

        public string SearchGroupKey { get; set; }

        public RetryQueueStatusTestModel Status { get; set; }
    }
}