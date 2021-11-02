namespace KafkaFlow.Retry.Durable
{
    using System.Threading.Tasks;
    using KafkaFlow.Retry.Durable.Encoders;

    internal class RetryDurableConsumerUtf8EncoderMiddleware : IMessageMiddleware
    {
        private readonly IUtf8Encoder utf8Encoder;

        public RetryDurableConsumerUtf8EncoderMiddleware(IUtf8Encoder utf8Encoder)
        {
            this.utf8Encoder = utf8Encoder;
        }

        public async Task Invoke(IMessageContext context, MiddlewareDelegate next)
        {
            await next(context.SetMessage(context.Message.Key, this.utf8Encoder.Decode((byte[])context.Message.Value))).ConfigureAwait(false);
        }
    }
}