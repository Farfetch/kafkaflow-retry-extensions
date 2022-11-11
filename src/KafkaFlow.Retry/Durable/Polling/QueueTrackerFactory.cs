﻿namespace KafkaFlow.Retry.Durable.Polling
{
    using Dawn;
    using KafkaFlow.Retry.Durable.Definitions;
    using KafkaFlow.Retry.Durable.Encoders;
    using KafkaFlow.Retry.Durable.Repository;
    using KafkaFlow.Retry.Durable.Repository.Adapters;

    internal class QueueTrackerFactory : IQueueTrackerFactory
    {
        private readonly IMessageAdapter messageAdapter;
        private readonly IMessageHeadersAdapter messageHeadersAdapter;
        private readonly IRetryDurableQueueRepository retryDurableQueueRepository;
        private readonly IUtf8Encoder utf8Encoder;

        public QueueTrackerFactory(
            IRetryDurableQueueRepository retryDurableQueueRepository,
            IMessageHeadersAdapter messageHeadersAdapter,
            IMessageAdapter messageAdapter,
            IUtf8Encoder utf8Encoder
        )
        {
            Guard.Argument(retryDurableQueueRepository).NotNull();
            Guard.Argument(messageHeadersAdapter).NotNull();
            Guard.Argument(messageAdapter).NotNull();
            Guard.Argument(utf8Encoder).NotNull();

            this.retryDurableQueueRepository = retryDurableQueueRepository;
            this.messageHeadersAdapter = messageHeadersAdapter;
            this.messageAdapter = messageAdapter;
            this.utf8Encoder = utf8Encoder;
        }

        public QueueTracker Create(
            RetryDurablePollingDefinition retryDurablePollingDefinition,
            IMessageProducer retryDurableMessageProducer,
            ILogHandler logHandler)
        {
            return new QueueTracker(
                logHandler,
                retryDurablePollingDefinition,
                new JobDetailProvider(
                    this.retryDurableQueueRepository,
                    logHandler,
                    this.messageHeadersAdapter,
                    this.messageAdapter,
                    this.utf8Encoder,
                    retryDurableMessageProducer,
                    retryDurablePollingDefinition),
                new TriggerProvider(retryDurablePollingDefinition));
        }
    }
}