namespace KafkaFlow.Retry.Durable.Common
{
    using System.IO;
    using System.IO.Compression;
    using Dawn;

    internal static class ByteArrayCompressionUtility
    {
        /// <summary>
        /// 64 KB
        /// </summary>
        private static int BUFFER_SIZE = 64 * 1024;

        public static byte[] Compress(byte[] data)
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

        public static byte[] Decompress(byte[] data)
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