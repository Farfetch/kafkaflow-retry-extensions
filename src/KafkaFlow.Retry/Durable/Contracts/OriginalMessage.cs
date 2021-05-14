namespace KafkaFlow.Retry.Durable.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    internal class OriginalMessage
    {
        public IEnumerable<OriginalMessageHeader> Headers { get; set; }
        public byte[] Key { get; set; }
        public long Offset { get; set; }
        public int Partition { get; set; }
        public string TopicName { get; set; }
        public DateTime UtcTimeStamp { get; set; }
        public byte[] Value { get; set; }
    }
}