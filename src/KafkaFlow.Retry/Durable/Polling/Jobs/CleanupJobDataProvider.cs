﻿using Dawn;
using KafkaFlow.Retry.Durable.Definitions.Polling;
using KafkaFlow.Retry.Durable.Repository;
using Quartz;

namespace KafkaFlow.Retry.Durable.Polling.Jobs;

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
        jobDetail = JobBuilder
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

    public IJobDetail JobDetail => jobDetail;

    public PollingDefinition PollingDefinition => cleanupPollingDefinition;

    public ITrigger Trigger => trigger;
}