using System.Diagnostics.CodeAnalysis;

namespace KafkaFlow.Retry.Durable.Repository.Model;

[ExcludeFromCodeCoverage]
public class MessageHeader
{
    public MessageHeader(string key, byte[] value)
    {
            this.Key = key;
            this.Value = value;
        }

    public string Key { get; }

    public byte[] Value { get; }
}