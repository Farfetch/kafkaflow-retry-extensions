namespace KafkaFlow.Retry.Durable.Repository.Adapters
{
    using System;
    using KafkaFlow.Retry.Durable.Compression;
    using KafkaFlow.Retry.Durable.Encoders;
    using KafkaFlow.Retry.Durable.Serializers;

    internal class MessageAdapter : IMessageAdapter
    {
        private readonly IGzipCompressor gzipCompressor;
        private readonly INewtonsoftJsonSerializer newtonsoftJsonSerializer;
        private readonly IUtf8Encoder utf8Encoder;

        public MessageAdapter(
            IGzipCompressor gzipCompressor,
            INewtonsoftJsonSerializer newtonsoftJsonSerializer,
            IUtf8Encoder utf8Encoder)
        {
            this.gzipCompressor = gzipCompressor;
            this.newtonsoftJsonSerializer = newtonsoftJsonSerializer;
            this.utf8Encoder = utf8Encoder;
        }

        public byte[] AdaptFromKafkaFlowMessage(object message)
        {
            var messageSerialized = this.newtonsoftJsonSerializer.SerializeObject(message);
            var messageEncoded = this.utf8Encoder.Encode(messageSerialized);
            var messageCompressed = this.gzipCompressor.Compress(messageEncoded);

            return messageCompressed;
        }

        public object AdaptToKafkaFlowMessage(byte[] message, Type type)
        {
            var messageDecompressed = this.gzipCompressor.Decompress(message);
            var messageDencoded = this.utf8Encoder.Decode(messageDecompressed);
            var messageDeserialized = this.newtonsoftJsonSerializer.DeserializeObject(messageDencoded, type);

            return messageDeserialized;
        }
    }
}