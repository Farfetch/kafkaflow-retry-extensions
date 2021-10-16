namespace KafkaFlow.Retry.Durable.Serializers
{
    using System;

    internal interface INewtonsoftJsonSerializer
    {
        object DeserializeObject(string data, Type type);

        string SerializeObject(object data);
    }
}