namespace KafkaFlow.Retry.Durable
{
    using System.Threading.Tasks;
    using Dawn;
    using KafkaFlow.Retry.Durable.Compression;

    internal class RetryDurableConsumerCompressorMiddleware : IMessageMiddleware
    {
        private readonly IGzipCompressor gzipCompressor;

        public RetryDurableConsumerCompressorMiddleware(IGzipCompressor gzipCompressor)
        {
            Guard.Argument(gzipCompressor).NotNull();

            this.gzipCompressor = gzipCompressor;
        }

        public async Task Invoke(IMessageContext context, MiddlewareDelegate next)
        {
            await next(context.SetMessage(context.Message.Key, this.gzipCompressor.Decompress((byte[])context.Message.Value))).ConfigureAwait(false);
        }
    }
}