namespace KafkaFlow.Retry.Durable.Common
{
    using Newtonsoft.Json;

    internal static class RetryDefaultSerializer
    {
        public static T DeserializeObject<T>(this byte[] data, bool decompress = false)
        {
            if (decompress)
            {
                data = ByteArrayCompressionUtility.Decompress(data);
            }

            T obj = JsonConvert.DeserializeObject<T>(System.Text.UTF8Encoding.UTF8.GetString(data));

            return obj;
        }

        public static byte[] SerializeObjectToJson(this object obj, bool compress = false)
        {
            byte[] serializedObj = System.Text.UTF8Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj));

            if (compress)
            {
                serializedObj = ByteArrayCompressionUtility.Compress(serializedObj);
            }

            return serializedObj;
        }
    }
}