using System.Runtime.Serialization;

namespace KafkaFlow.Retry.Sample.Messages;

[DataContract]
public class RetryForeverTestMessage
{
    [DataMember(Order = 1)] public string Text { get; set; }
}