using System;

namespace KafkaFlow.Retry.Durable.Serializers;

internal interface INewtonsoftJsonSerializer
{
    object DeserializeObject(string data, Type type);

    string SerializeObject(object data);
}