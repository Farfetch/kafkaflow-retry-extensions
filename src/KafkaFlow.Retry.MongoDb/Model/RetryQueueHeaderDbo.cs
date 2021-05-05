namespace KafkaFlow.Retry.MongoDb.Model
{
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    public class RetryQueueHeaderDbo
    {
        public string Key { get; set; }

        public byte[] Value { get; set; }
    }
}
