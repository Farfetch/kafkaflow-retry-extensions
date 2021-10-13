namespace KafkaFlow.Retry.Sample.Messages
{
    using System.Runtime.Serialization;

    [DataContract]
    public class RetryForeverTestMessage
    {
        [DataMember(Order = 1)]
        public string Text { get; set; }
    }
}