namespace KafkaFlow.Retry.Durable
{
    using System.Threading.Tasks;

    internal class KafkaRetryDurableValidationMiddleware : IMessageMiddleware
    {
        private readonly ILogHandler logHandler;

        public KafkaRetryDurableValidationMiddleware(
            ILogHandler logHandler)
        {
            this.logHandler = logHandler;
        }

        public Task Invoke(IMessageContext context, MiddlewareDelegate next)
        {
            this.logHandler.Info("KafkaRetryDurableValidationMiddleware", new { });
            next(context);
            return Task.CompletedTask;
        }
    }
}