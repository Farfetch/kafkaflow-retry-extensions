namespace KafkaFlow.Retry.API.Dtos.Common
{
    using System;

    public class RetryQueuetItemMessageInfoDto
    {
        public byte[] Key { get; set; }

        public long Offset { get; set; }

        public int Partition { get; set; }

        public string Topic { get; set; }

        public DateTimeOffset UtcTimeStamp { get; set; }
    }
}
