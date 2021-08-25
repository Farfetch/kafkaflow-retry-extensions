namespace KafkaFlow.Retry.IntegrationTests.Core.Messages
{
    using System.Runtime.Serialization;

    [DataContract]
    public class RetryDurableTestMessage
    {
        [DataMember(Order = 1)]
        public string Key { get; set; }

        [DataMember(Order = 2)]
        public string Value { get; set; }
    }
}