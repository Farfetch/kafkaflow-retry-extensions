namespace KafkaFlow.Retry.Durable.Polling
{
    using Dawn;
    using KafkaFlow.Producers;
    using KafkaFlow.Retry.Durable.Definitions;
    using KafkaFlow.Retry.Durable.Encoders;
    using KafkaFlow.Retry.Durable.Repository;
    using KafkaFlow.Retry.Durable.Repository.Adapters;

    internal class QueueTrackerFactory : IQueueTrackerFactory
    {
        private readonly ILogHandler logHandler;
        private readonly IMessageAdapter messageAdapter;
        private readonly IMessageHeadersAdapter messageHeadersAdapter;
        private readonly IMessageProducer retryDurableMessageProducer;
        private readonly RetryDurablePollingDefinition retryDurablePollingDefinition;
        private readonly IRetryDurableQueueRepository retryDurableQueueRepository;
        private readonly IUtf8Encoder utf8Encoder;

        public QueueTrackerFactory(
            IRetryDurableQueueRepository retryDurableQueueRepository,
            ILogHandler logHandler,
            IMessageHeadersAdapter messageHeadersAdapter,
            IMessageAdapter messageAdapter,
            IUtf8Encoder utf8Encoder,
            IMessageProducer retryDurableMessageProducer,
            RetryDurablePollingDefinition retryDurablePollingDefinition
        )
        {
            Guard.Argument(retryDurableQueueRepository).NotNull();
            Guard.Argument(logHandler).NotNull();
            Guard.Argument(messageHeadersAdapter).NotNull();
            Guard.Argument(messageAdapter).NotNull();
            Guard.Argument(utf8Encoder).NotNull();
            Guard.Argument(retryDurableMessageProducer).NotNull();
            Guard.Argument(retryDurablePollingDefinition).NotNull();

            this.retryDurableQueueRepository = retryDurableQueueRepository;
            this.logHandler = logHandler;
            this.messageHeadersAdapter = messageHeadersAdapter;
            this.messageAdapter = messageAdapter;
            this.utf8Encoder = utf8Encoder;
            this.retryDurableMessageProducer = retryDurableMessageProducer;
            this.retryDurablePollingDefinition = retryDurablePollingDefinition;
        }

        public QueueTracker Create()
        {
            return new QueueTracker(
                this.retryDurableQueueRepository,
                this.logHandler,
                this.messageHeadersAdapter,
                this.messageAdapter,
                this.utf8Encoder,
                this.retryDurableMessageProducer,
                this.retryDurablePollingDefinition
                );
        }
    }
}