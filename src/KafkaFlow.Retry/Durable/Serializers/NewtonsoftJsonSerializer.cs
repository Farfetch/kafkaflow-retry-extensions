namespace KafkaFlow.Retry.Durable.Serializers
{
    using System;
    using Newtonsoft.Json;

    internal class NewtonsoftJsonSerializer : INewtonsoftJsonSerializer
    {
        private readonly JsonSerializerSettings jsonSerializerSettings;

        public NewtonsoftJsonSerializer()
           : this(new JsonSerializerSettings())
        { }

        public NewtonsoftJsonSerializer(JsonSerializerSettings jsonSerializerSettings)
        {
            this.jsonSerializerSettings = jsonSerializerSettings;
        }

        public object DeserializeObject(string data, Type type)
        {
            return JsonConvert.DeserializeObject(data, type, jsonSerializerSettings);
        }

        public string SerializeObject(object data)
        {
            return JsonConvert.SerializeObject(data, jsonSerializerSettings);
        }
    }
}