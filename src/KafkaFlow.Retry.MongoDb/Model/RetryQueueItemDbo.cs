namespace KafkaFlow.Retry.MongoDb.Model
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using KafkaFlow.Retry.Durable.Common;
    using KafkaFlow.Retry.Durable.Repository.Model;

    [ExcludeFromCodeCoverage]
    public class RetryQueueItemDbo
    {
        public int AttemptsCount { get; set; }

        public DateTime CreationDate { get; set; }

        public string Description { get; set; }
        public Guid Id { get; set; }
        public DateTime? LastExecution { get; set; }
        public RetryQueueItemMessageDbo Message { get; set; }
        public DateTime? ModifiedStatusDate { get; set; }
        public Guid RetryQueueId { get; set; }

        public SeverityLevel SeverityLevel { get; set; }

        public int Sort { get; set; }

        public RetryQueueItemStatus Status { get; set; }
    }
}