using KafkaFlow.Retry.Durable.Compression;
using KafkaFlow.Retry.Durable.Encoders;
using KafkaFlow.Retry.Durable.Serializers;

namespace KafkaFlow.Retry.Durable.Repository.Adapters;

internal class NewtonsoftJsonMessageAdapter : IMessageAdapter
{
    private readonly IGzipCompressor _gzipCompressor;
    private readonly INewtonsoftJsonSerializer _newtonsoftJsonSerializer;
    private readonly IUtf8Encoder _utf8Encoder;

    public NewtonsoftJsonMessageAdapter(
        IGzipCompressor gzipCompressor,
        INewtonsoftJsonSerializer newtonsoftJsonSerializer,
        IUtf8Encoder utf8Encoder)
    {
            _gzipCompressor = gzipCompressor;
            _newtonsoftJsonSerializer = newtonsoftJsonSerializer;
            _utf8Encoder = utf8Encoder;
        }

    public byte[] AdaptMessageToRepository(object message)
    {
            var messageSerialized = _newtonsoftJsonSerializer.SerializeObject(message);
            var messageEncoded = _utf8Encoder.Encode(messageSerialized);
            return _gzipCompressor.Compress(messageEncoded);
        }
}