namespace KafkaFlow.Retry.Durable.Polling
{
    using Dawn;
    using KafkaFlow.Retry.Durable.Definitions;

    internal class QueueTrackerCoordinator : IQueueTrackerCoordinator
    {
        private readonly IQueueTrackerFactory queueTrackerFactory;
        private readonly RetryDurablePollingDefinition retryDurablePollingDefinition;
        private QueueTracker queueTracker;

        public QueueTrackerCoordinator(
            IQueueTrackerFactory queueTrackerFactory,
            RetryDurablePollingDefinition retryDurablePollingDefinition)
        {
            Guard.Argument(queueTrackerFactory).NotNull();
            Guard.Argument(retryDurablePollingDefinition).NotNull("No polling definitions was found");

            this.queueTrackerFactory = queueTrackerFactory;
            this.retryDurablePollingDefinition = retryDurablePollingDefinition;
        }

        public void Initialize()
        {
            if (!retryDurablePollingDefinition.Enabled)
            {
                return;
            }

            this.queueTracker = this.queueTrackerFactory.Create();
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