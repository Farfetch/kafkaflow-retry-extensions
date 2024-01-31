using System.Threading.Tasks;
using Dawn;
using KafkaFlow.Retry.Durable.Compression;

namespace KafkaFlow.Retry.Durable;

internal class RetryDurableConsumerCompressorMiddleware : IMessageMiddleware
{
    private readonly IGzipCompressor _gzipCompressor;

    public RetryDurableConsumerCompressorMiddleware(IGzipCompressor gzipCompressor)
    {
        Guard.Argument(gzipCompressor).NotNull();

        _gzipCompressor = gzipCompressor;
    }

    public async Task Invoke(IMessageContext context, MiddlewareDelegate next)
    {
        await next(context.SetMessage(context.Message.Key, _gzipCompressor.Decompress((byte[])context.Message.Value)))
            .ConfigureAwait(false);
    }
}