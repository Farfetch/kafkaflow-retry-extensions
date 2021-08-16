namespace KafkaFlow.Retry.Durable.Polling
{
    using Dawn;
    using KafkaFlow.Retry.Durable.Definitions;

    internal class QueueTrackerCoordinator : IQueueTrackerCoordinator
    {
        private readonly IQueueTrackerFactory queueTrackerFactory;
        private QueueTracker queueTracker;

        public QueueTrackerCoordinator(IQueueTrackerFactory queueTrackerFactory)
        {
            Guard.Argument(queueTrackerFactory).NotNull();

            this.queueTrackerFactory = queueTrackerFactory;
        }

        public void Initialize(
            RetryDurablePollingDefinition retryDurablePollingDefinition,
            IMessageProducer retryDurableMessageProducer,
            ILogHandler logHandler)
        {
            if (!retryDurablePollingDefinition.Enabled)
            {
                return;
            }

            this.queueTracker = this.queueTrackerFactory
                .Create(
                    retryDurablePollingDefinition,
                    retryDurableMessageProducer,
                    logHandler);
            this.queueTracker.ScheduleJob();
        }

        public void Shutdown()
        {
            if (this.queueTracker is object)
            {
                this.queueTracker.Shutdown();
            }
        }
    }
}