namespace KafkaFlow.Retry.Durable.Common
{
    using System;
    using System.Text;
    using Newtonsoft.Json;

    internal static class RetryDefaultSerializer
    {
        public static string ByteArrayToString(this byte[] data)
        {
            return UTF8Encoding.UTF8.GetString(data);
        }

        public static T DeserializeObject<T>(this byte[] data, bool decompress = false)
        {
            if (decompress)
            {
                data = ByteArrayCompressionUtility.Decompress(data);
            }

            T obj = JsonConvert.DeserializeObject<T>(data.ByteArrayToString());

            return obj;
        }

        public static object DeserializeObject(this byte[] data, Type type, bool decompress = false)
        {
            if (decompress)
            {
                data = ByteArrayCompressionUtility.Decompress(data);
            }

            var obj = JsonConvert.DeserializeObject(data.ByteArrayToString(), type);

            return obj;
        }

        public static byte[] SerializeObjectToJson(this object obj, bool compress = false)
        {
            byte[] serializedObj = JsonConvert.SerializeObject(obj).StringToByteArray();

            if (compress)
            {
                serializedObj = ByteArrayCompressionUtility.Compress(serializedObj);
            }

            return serializedObj;
        }

        public static byte[] StringToByteArray(this string data)
        {
            return UTF8Encoding.UTF8.GetBytes(data);
        }
    }
}