namespace KafkaFlow.Retry.Durable.Polling.Jobs
{
    using Dawn;
    using KafkaFlow.Retry.Durable.Definitions.Polling;
    using KafkaFlow.Retry.Durable.Repository;
    using Quartz;

    internal class CleanupJobDataProvider : IJobDataProvider
    {
        private readonly CleanupPollingDefinition cleanupPollingDefinition;
        private readonly IJobDetail jobDetail;
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
            this.jobDetail = JobBuilder
                .Create<CleanupPollingJob>()
                .WithIdentity($"pollingJob_{schedulerId}_{cleanupPollingDefinition.PollingJobType}", "queueTrackerGroup")
                .SetJobData(
                    new JobDataMap
                    {
                        { PollingJobConstants.CleanupPollingDefinition, cleanupPollingDefinition },
                        { PollingJobConstants.SchedulerId, schedulerId },
                        { PollingJobConstants.RetryDurableQueueRepository, retryDurableQueueRepository },
                        { PollingJobConstants.LogHandler, logHandler }
                    })
                .Build();
        }

        public IJobDetail JobDetail => this.jobDetail;

        public PollingDefinition PollingDefinition => this.cleanupPollingDefinition;

        public ITrigger Trigger => this.trigger;
    }
}