namespace KafkaFlow.Retry.Durable.Repository.Adapters
{
    using KafkaFlow.Retry.Durable.Compression;
    using KafkaFlow.Retry.Durable.Encoders;
    using KafkaFlow.Retry.Durable.Serializers;

    internal class NewtonsoftJsonMessageAdapter : IMessageAdapter
    {
        private readonly IGzipCompressor gzipCompressor;
        private readonly INewtonsoftJsonSerializer newtonsoftJsonSerializer;
        private readonly IUtf8Encoder utf8Encoder;

        public NewtonsoftJsonMessageAdapter(
            IGzipCompressor gzipCompressor,
            INewtonsoftJsonSerializer newtonsoftJsonSerializer,
            IUtf8Encoder utf8Encoder)
        {
            this.gzipCompressor = gzipCompressor;
            this.newtonsoftJsonSerializer = newtonsoftJsonSerializer;
            this.utf8Encoder = utf8Encoder;
        }

        public byte[] AdaptMessageToRepository(object message)
        {
            var messageSerialized = this.newtonsoftJsonSerializer.SerializeObject(message);
            var messageEncoded = this.utf8Encoder.Encode(messageSerialized);
            return this.gzipCompressor.Compress(messageEncoded);
        }
    }
}