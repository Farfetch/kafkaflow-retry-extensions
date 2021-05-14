namespace KafkaFlow.Retry.Durable.Polling
{
    using KafkaFlow.Consumers;
    using KafkaFlow.Producers;
    using KafkaFlow.Retry.Durable.Repository;

    internal class QueueTrackerFactory : IQueueTrackerFactory
    {
        private readonly IMessageConsumer messageConsumer;
        private readonly IMessageProducer messageProducer;
        private readonly IKafkaRetryDurableQueueRepository queueStorage;

        public QueueTrackerFactory(
            IKafkaRetryDurableQueueRepository queueStorage,
            IMessageProducer messageProducer,
            IMessageConsumer messageConsumer
        )
        {
            this.queueStorage = queueStorage;
            this.messageProducer = messageProducer;
            this.messageConsumer = messageConsumer;
        }

        public QueueTracker Create()
        {
            return new QueueTracker(this.queueStorage, this.messageProducer, this.messageConsumer);
        }
    }
}