namespace KafkaFlow.Retry.Postgres.Model
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using KafkaFlow.Retry.Durable.Repository.Model;
    
    [ExcludeFromCodeCoverage]
    internal class RetryQueueDbo
    {
        public DateTime CreationDate { get; set; }

        public long Id { get; set; }

        public Guid IdDomain { get; set; }

        public DateTime LastExecution { get; set; }

        public string QueueGroupKey { get; set; }

        public string SearchGroupKey { get; set; }

        public RetryQueueStatus Status { get; set; }
    }
}
