namespace KafkaFlow.Retry.Durable.Serializers
{
    using System;
    using Newtonsoft.Json;

    internal class NewtonsoftJsonSerializer : INewtonsoftJsonSerializer
    {
        public object DeserializeObject(string data, Type type)
        {
            return JsonConvert.DeserializeObject(data, type);
        }

        public string SerializeObject(object data)
        {
            return JsonConvert.SerializeObject(data);
        }
    }
}