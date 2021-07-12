namespace KafkaFlow.Retry.Durable.Serializers
{
    internal interface IProtobufNetSerializer
    {
        byte[] Serialize(object data);
    }
}