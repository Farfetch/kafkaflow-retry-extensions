namespace KafkaFlow.Retry.Durable.Polling.Jobs
{
    using Dawn;
    using KafkaFlow.Retry.Durable.Definitions.Polling;
    using KafkaFlow.Retry.Durable.Repository;
    using Quartz;

    internal class CleanupJobDataProvider : IJobDataProvider
    {
        private readonly CleanupPollingDefinition cleanupPollingDefinition;
        private readonly ILogHandler logHandler;
        private readonly IRetryDurableQueueRepository retryDurableQueueRepository;
        private readonly string schedulerId;
        private readonly ITrigger trigger;

        public CleanupJobDataProvider(
            CleanupPollingDefinition cleanupPollingDefinition,
            ITrigger trigger,
            string schedulerId,
            IRetryDurableQueueRepository retryDurableQueueRepository,
            ILogHandler logHandler)
        {
            Guard.Argument(cleanupPollingDefinition, nameof(cleanupPollingDefinition)).NotNull();
            Guard.Argument(trigger, nameof(trigger)).NotNull();
            Guard.Argument(schedulerId, nameof(schedulerId)).NotNull().NotEmpty();
            Guard.Argument(retryDurableQueueRepository, nameof(retryDurableQueueRepository)).NotNull();
            Guard.Argument(logHandler, nameof(logHandler)).NotNull();

            this.cleanupPollingDefinition = cleanupPollingDefinition;
            this.trigger = trigger;
            this.schedulerId = schedulerId;
            this.retryDurableQueueRepository = retryDurableQueueRepository;
            this.logHandler = logHandler;
        }

        public PollingDefinition PollingDefinition => this.cleanupPollingDefinition;

        public ITrigger Trigger => this.trigger;

        public IJobDetail GetPollingJobDetail()
        {
            var dataMap = new JobDataMap();

            dataMap.Add(PollingJobConstants.CleanupPollingDefinition, this.cleanupPollingDefinition);
            dataMap.Add(PollingJobConstants.SchedulerId, this.schedulerId);
            dataMap.Add(PollingJobConstants.RetryDurableQueueRepository, this.retryDurableQueueRepository);
            dataMap.Add(PollingJobConstants.LogHandler, this.logHandler);

            return JobBuilder
                .Create<CleanupPollingJob>()
                .SetJobData(dataMap)
                .Build();
        }
    }
}