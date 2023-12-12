namespace KafkaFlow.Retry.Durable.Compression;

internal interface IGzipCompressor
{
    byte[] Compress(byte[] data);

    byte[] Decompress(byte[] data);
}