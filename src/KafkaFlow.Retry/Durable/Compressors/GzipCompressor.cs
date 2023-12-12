using System.IO;
using System.IO.Compression;
using Dawn;

namespace KafkaFlow.Retry.Durable.Compression;

internal class GzipCompressor : IGzipCompressor
{
    private static readonly int s_bufferSize = 64 * 1024;

    public byte[] Compress(byte[] data)
    {
        Guard.Argument(data).NotNull();

        using (var compressIntoMs = new MemoryStream())
        {
            using (var gzs = new BufferedStream(new GZipStream(compressIntoMs, CompressionLevel.Fastest), s_bufferSize))
            {
                gzs.Write(data, 0, data.Length);
            }

            return compressIntoMs.ToArray();
        }
    }

    public byte[] Decompress(byte[] data)
    {
        Guard.Argument(data).NotNull();

        using (var compressedMs = new MemoryStream(data))
        {
            using (var decompressedMs = new MemoryStream())
            {
                using (var gzs = new BufferedStream(new GZipStream(compressedMs, CompressionMode.Decompress),
                           s_bufferSize))
                {
                    gzs.CopyTo(decompressedMs);
                }

                return decompressedMs.ToArray();
            }
        }
    }
}