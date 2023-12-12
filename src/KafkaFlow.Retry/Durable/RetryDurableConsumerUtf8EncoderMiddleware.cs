using System.Threading.Tasks;
using Dawn;
using KafkaFlow.Retry.Durable.Encoders;

namespace KafkaFlow.Retry.Durable;

internal class RetryDurableConsumerUtf8EncoderMiddleware : IMessageMiddleware
{
    private readonly IUtf8Encoder _utf8Encoder;

    public RetryDurableConsumerUtf8EncoderMiddleware(IUtf8Encoder utf8Encoder)
    {
            Guard.Argument(utf8Encoder).NotNull();

            _utf8Encoder = utf8Encoder;
        }

    public async Task Invoke(IMessageContext context, MiddlewareDelegate next)
    {
            await next(context.SetMessage(context.Message.Key, _utf8Encoder.Decode((byte[])context.Message.Value))).ConfigureAwait(false);
        }
}