using System;
using Newtonsoft.Json;

namespace KafkaFlow.Retry.Durable.Serializers;

internal class NewtonsoftJsonSerializer : INewtonsoftJsonSerializer
{
    private readonly JsonSerializerSettings _jsonSerializerSettings;

    public NewtonsoftJsonSerializer()
        : this(new JsonSerializerSettings())
    {
    }

    public NewtonsoftJsonSerializer(JsonSerializerSettings jsonSerializerSettings)
    {
        _jsonSerializerSettings = jsonSerializerSettings;
    }

    public object DeserializeObject(string data, Type type)
    {
        return JsonConvert.DeserializeObject(data, type, _jsonSerializerSettings);
    }

    public string SerializeObject(object data)
    {
        return JsonConvert.SerializeObject(data, _jsonSerializerSettings);
    }
}