using System.Runtime.Serialization;

namespace KafkaFlow.Retry.IntegrationTests.Core.Messages;

[DataContract]
internal class RetryForeverTestMessage : ITestMessage
{
    [DataMember(Order = 1)] public string Key { get; set; }

    [DataMember(Order = 2)] public string Value { get; set; }
}