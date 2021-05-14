namespace KafkaFlow.Retry.Durable.Contracts
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    internal class InRetryMessage
    {
        public int AttemptsCount { get; set; }
        public Guid ItemId { get; set; }
        public OriginalMessage OriginalMessage { get; set; }
        public Guid QueueId { get; set; }
        public int Sort { get; set; }
        public string Version => "v1";
    }
}