namespace KafkaFlow.Retry
{
    using System.Threading.Tasks;
    using KafkaFlow;

    public class KafkaRetryMiddleware : IMessageMiddleware
    {
        private readonly ILogHandler logHandler;

        public KafkaRetryMiddleware(ILogHandler logHandler)
        {
            this.logHandler = logHandler;
        }

        public Task Invoke(IMessageContext context, MiddlewareDelegate next)
        {
            this.logHandler.Info("This is just the beginning...", new object { });
            return next(context);
        }
    }
}