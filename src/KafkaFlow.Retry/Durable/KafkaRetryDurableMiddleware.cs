namespace KafkaFlow.Retry.Durable
{
    using System.Threading.Tasks;
    using KafkaFlow;

    internal class KafkaRetryDurableMiddleware : IMessageMiddleware
    {
        private readonly KafkaRetryDurableDefinition kafkaRetryDurableDefinition;
        private readonly ILogHandler logHandler;
        private readonly object syncPauseAndResume = new object();
        private int? controlWorkerId;

        public KafkaRetryDurableMiddleware(
            ILogHandler logHandler,
            KafkaRetryDurableDefinition kafkaRetryForeverDefinition)
        {
            this.logHandler = logHandler;
            this.kafkaRetryDurableDefinition = kafkaRetryForeverDefinition;
        }

        public Task Invoke(IMessageContext context, MiddlewareDelegate next)
        {
            return Task.CompletedTask;
        }
    }
}