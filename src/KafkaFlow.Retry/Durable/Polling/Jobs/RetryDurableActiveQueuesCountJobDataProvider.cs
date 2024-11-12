using System;
using System.Collections.Generic;
using System.Text;
using Dawn;
using KafkaFlow.Retry.Durable.Definitions.Polling;
using KafkaFlow.Retry.Durable.Repository;
using Quartz;

namespace KafkaFlow.Retry.Durable.Polling.Jobs;

internal class RetryDurableActiveQueuesCountJobDataProvider : IJobDataProvider
{
    private readonly IJobDetail _jobDetail;
    private readonly RetryDurableActiveQueuesCountPollingDefinition _retryDurableActiveQueuesCountPollingDefinition;
    private readonly ITrigger _trigger;

    public RetryDurableActiveQueuesCountJobDataProvider(
        RetryDurableActiveQueuesCountPollingDefinition retryDurableActiveQueuesCountPollingDefinition,
        ITrigger trigger,
        string schedulerId,
        IRetryDurableQueueRepository retryDurableQueueRepository,
        ILogHandler logHandler)
    {
        Guard.Argument(retryDurableActiveQueuesCountPollingDefinition, nameof(retryDurableActiveQueuesCountPollingDefinition)).NotNull();
        Guard.Argument(trigger, nameof(trigger)).NotNull();
        Guard.Argument(schedulerId, nameof(schedulerId)).NotNull().NotEmpty();
        Guard.Argument(retryDurableQueueRepository, nameof(retryDurableQueueRepository)).NotNull();
        Guard.Argument(logHandler, nameof(logHandler)).NotNull();

        _retryDurableActiveQueuesCountPollingDefinition = retryDurableActiveQueuesCountPollingDefinition;
        _trigger = trigger;
        _jobDetail = JobBuilder
            .Create<RetryDurableActiveQueuesCountJob>()
            .WithIdentity($"pollingJob_{schedulerId}_{retryDurableActiveQueuesCountPollingDefinition.PollingJobType}", "queueTrackerGroup")
            .SetJobData(
                new JobDataMap
                {
                    { PollingJobConstants.RetryDurableActiveQueuesCountPollingDefinition, retryDurableActiveQueuesCountPollingDefinition },
                    { PollingJobConstants.SchedulerId, schedulerId },
                    { PollingJobConstants.RetryDurableQueueRepository, retryDurableQueueRepository },
                    { PollingJobConstants.LogHandler, logHandler }
                })
            .Build();
    }

    public IJobDetail JobDetail => _jobDetail;

    public PollingDefinition PollingDefinition => _retryDurableActiveQueuesCountPollingDefinition;

    public ITrigger Trigger => _trigger;
}
