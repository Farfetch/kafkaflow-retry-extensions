namespace KafkaFlow.Retry.MongoDb.Model
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    public class RetryQueueItemMessageDbo
    {
        public IEnumerable<RetryQueueHeaderDbo> Headers { get; set; }

        public byte[] Key { get; set; }

        public long Offset { get; set; }

        public int Partition { get; set; }

        public string TopicName { get; set; }

        public DateTime UtcTimeStamp { get; set; }

        public byte[] Value { get; set; }
    }
}
