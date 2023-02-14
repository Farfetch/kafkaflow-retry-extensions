namespace KafkaFlow.Retry.Durable.Polling
{
    using Dawn;

    internal class QueueTrackerCoordinator : IQueueTrackerCoordinator
    {
        private readonly IQueueTrackerFactory queueTrackerFactory;
        private QueueTracker queueTracker;

        public QueueTrackerCoordinator(IQueueTrackerFactory queueTrackerFactory)
        {
            Guard.Argument(queueTrackerFactory).NotNull();

            this.queueTrackerFactory = queueTrackerFactory;
        }

        public void ScheduleJobs(IMessageProducer retryDurableMessageProducer, ILogHandler logHandler)
        {
            this.queueTracker = this.queueTrackerFactory
                .Create(retryDurableMessageProducer, logHandler);

            this.queueTracker.ScheduleJobs();
        }

        public void UnscheduleJobs()
        {
            if (this.queueTracker is object)
            {
                this.queueTracker.UnscheduleJobs();
            }
        }
    }
}