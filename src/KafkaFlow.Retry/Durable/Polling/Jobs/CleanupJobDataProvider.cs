using Dawn;
using KafkaFlow.Retry.Durable.Definitions.Polling;
using KafkaFlow.Retry.Durable.Repository;
using Quartz;

namespace KafkaFlow.Retry.Durable.Polling.Jobs
{
    internal class CleanupJobDataProvider : IJobDataProvider
    {
        private readonly CleanupPollingDefinition cleanupPollingDefinition;
        private readonly ILogHandler logHandler;
        private readonly IRetryDurableQueueRepository retryDurableQueueRepository;
        private readonly string schedulerId;

        public CleanupJobDataProvider(
            IRetryDurableQueueRepository retryDurableQueueRepository,
            ILogHandler logHandler,
            CleanupPollingDefinition cleanupPollingDefinition,
            string schedulerId)
        {
            Guard.Argument(retryDurableQueueRepository, nameof(retryDurableQueueRepository)).NotNull();
            Guard.Argument(logHandler, nameof(logHandler)).NotNull();
            Guard.Argument(cleanupPollingDefinition, nameof(cleanupPollingDefinition)).NotNull();
            Guard.Argument(schedulerId, nameof(schedulerId)).NotNull().NotEmpty();

            this.retryDurableQueueRepository = retryDurableQueueRepository;
            this.logHandler = logHandler;
            this.cleanupPollingDefinition = cleanupPollingDefinition;
            this.schedulerId = schedulerId;
        }

        public PollingDefinition PollingDefinition => this.cleanupPollingDefinition;

        public IJobDetail GetPollingJobDetail()
        {
            var dataMap = new JobDataMap();

            dataMap.Add(PollingJobConstants.SchedulerId, this.schedulerId);
            dataMap.Add(PollingJobConstants.RetryDurableQueueRepository, this.retryDurableQueueRepository);
            dataMap.Add(PollingJobConstants.CleanupPollingDefinition, this.cleanupPollingDefinition);
            dataMap.Add(PollingJobConstants.LogHandler, this.logHandler);

            return JobBuilder
                .Create<CleanupPollingJob>()
                .SetJobData(dataMap)
                .Build();
        }
    }
}