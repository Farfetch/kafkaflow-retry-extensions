﻿namespace KafkaFlow.Retry.Durable.Repository.Adapters
{
    using KafkaFlow.Retry.Durable.Compression;
    using KafkaFlow.Retry.Durable.Serializers;

    internal class MessageAdapter : IMessageAdapter
    {
        private readonly IGzipCompressor gzipCompressor;
        private readonly IProtobufNetSerializer protobufNetSerializer;

        public MessageAdapter(
            IGzipCompressor gzipCompressor,
            IProtobufNetSerializer protobufNetSerializer)
        {
            this.gzipCompressor = gzipCompressor;
            this.protobufNetSerializer = protobufNetSerializer;
        }

        public byte[] AdaptMessageFromRepository(byte[] message)
        {
            return this.gzipCompressor.Decompress(message);
        }

        public byte[] AdaptMessageToRepository(object message)
        {
            byte[] messageSerialized = this.protobufNetSerializer.Serialize(message);
            return this.gzipCompressor.Compress(messageSerialized);
        }
    }
}