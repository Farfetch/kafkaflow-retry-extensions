namespace KafkaFlow.Retry.Durable
{
    using System.Text;
    using System.Threading.Tasks;
    using KafkaFlow;
    using KafkaFlow.Producers;

    internal class KafkaRetryDurableMiddleware : IMessageMiddleware
    {
        private readonly KafkaRetryDurableDefinition kafkaRetryDurableDefinition;
        private readonly ILogHandler logHandler;
        private readonly IProducerAccessor producerAccessor;
        private readonly object syncPauseAndResume = new object();
        private int? controlWorkerId;

        public KafkaRetryDurableMiddleware(
            ILogHandler logHandler,
            KafkaRetryDurableDefinition kafkaRetryForeverDefinition,
            IProducerAccessor producerAccessor)
        {
            this.logHandler = logHandler;
            this.kafkaRetryDurableDefinition = kafkaRetryForeverDefinition;
            this.producerAccessor = producerAccessor;
        }

        public async Task Invoke(IMessageContext context, MiddlewareDelegate next)
        {
            await producerAccessor
                .GetProducer("kafka-flow-retry-durable-producer")
                .ProduceAsync(
                    "test-topic-retry",
                    Encoding.UTF8.GetString(context.PartitionKey),
                    context.Message,
                    null
                )
                .ConfigureAwait(false);

            next(context);
        }
    }
}