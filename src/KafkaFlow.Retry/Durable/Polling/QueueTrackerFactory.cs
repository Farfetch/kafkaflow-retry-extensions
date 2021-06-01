namespace KafkaFlow.Retry.Durable.Polling
{
    using KafkaFlow.Producers;
    using KafkaFlow.Retry.Durable.Polling.Strategies;
    using KafkaFlow.Retry.Durable.Repository;

    internal class QueueTrackerFactory : IQueueTrackerFactory
    {
        private readonly KafkaRetryDurablePollingDefinition kafkaRetryDurablePollingDefinition;
        private readonly IMessageProducer messageProducer;
        private readonly IKafkaRetryDurableQueueRepository queueStorage;

        public QueueTrackerFactory(
            IKafkaRetryDurableQueueRepository queueStorage,
            IMessageProducer messageProducer,
            KafkaRetryDurablePollingDefinition kafkaRetryDurablePollingDefinition
        )
        {
            this.queueStorage = queueStorage;
            this.messageProducer = messageProducer;
            this.kafkaRetryDurablePollingDefinition = kafkaRetryDurablePollingDefinition;
        }

        public QueueTracker Create()
        {
            return new QueueTracker(
                this.queueStorage, 
                this.messageProducer, 
                this.kafkaRetryDurablePollingDefinition,
                new PollingJobStrategyProvider()
                );
        }
    }
}