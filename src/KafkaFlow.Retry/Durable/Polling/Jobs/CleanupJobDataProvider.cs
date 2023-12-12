using Dawn;
using KafkaFlow.Retry.Durable.Definitions.Polling;
using KafkaFlow.Retry.Durable.Repository;
using Quartz;

namespace KafkaFlow.Retry.Durable.Polling.Jobs;

internal class CleanupJobDataProvider : IJobDataProvider
{
    private readonly CleanupPollingDefinition _cleanupPollingDefinition;
    private readonly IJobDetail _jobDetail;
    private readonly ITrigger _trigger;

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

        _cleanupPollingDefinition = cleanupPollingDefinition;
        _trigger = trigger;
        _jobDetail = JobBuilder
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

    public IJobDetail JobDetail => _jobDetail;

    public PollingDefinition PollingDefinition => _cleanupPollingDefinition;

    public ITrigger Trigger => _trigger;
}