namespace KafkaFlow.Retry.Durable.Serializers
{
    using Microsoft.IO;
    using ProtoBuf;

    internal class ProtobufNetSerializer : IProtobufNetSerializer
    {
        private static readonly RecyclableMemoryStreamManager MemoryStreamManager = new RecyclableMemoryStreamManager();

        public byte[] Serialize(object data)
        {
            byte[] messageSerialized;
            using (var output = MemoryStreamManager.GetStream())
            {
                Serializer.Serialize(output, data);

                messageSerialized = output.ToArray();
            }

            return messageSerialized;
        }
    }
}