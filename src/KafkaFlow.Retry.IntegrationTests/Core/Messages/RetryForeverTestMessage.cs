namespace KafkaFlow.Retry.IntegrationTests.Core.Messages
{
    using System.Runtime.Serialization;

    [DataContract]
    internal class RetryForeverTestMessage
    {
        [DataMember(Order = 1)]
        public string Key { get; set; }

        [DataMember(Order = 2)]
        public string Value { get; set; }
    }
}