namespace KafkaFlow.Retry.Durable.Compression
{
    using System.IO;
    using System.IO.Compression;
    using Dawn;

    internal class GzipCompressor : IGzipCompressor
    {
        private static int BUFFER_SIZE = 64 * 1024;

        public byte[] Compress(byte[] data)
        {
            Guard.Argument(data).NotNull();

            using (var compressIntoMs = new MemoryStream())
            {
                using (var gzs = new BufferedStream(new GZipStream(compressIntoMs, CompressionLevel.Fastest), BUFFER_SIZE))
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
                    using (var gzs = new BufferedStream(new GZipStream(compressedMs, CompressionMode.Decompress), BUFFER_SIZE))
                    {
                        gzs.CopyTo(decompressedMs);
                    }
                    return decompressedMs.ToArray();
                }
            }
        }
    }
}