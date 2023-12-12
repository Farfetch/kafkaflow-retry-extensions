using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using Dawn;
using Quartz;
using Quartz.Impl;

namespace KafkaFlow.Retry.Durable.Polling;

internal class QueueTracker
{
    private static readonly object internalLock = new object();
    private readonly IEnumerable<IJobDataProvider> jobDataProviders;
    private readonly ILogHandler logHandler;
    private readonly string schedulerId;
    private IScheduler scheduler;

    public QueueTracker(
        string schedulerId,
        IEnumerable<IJobDataProvider> jobDataProviders,
        ILogHandler logHandler
    )
    {
            Guard.Argument(schedulerId, nameof(schedulerId)).NotNull().NotEmpty();
            Guard.Argument(jobDataProviders).NotNull().NotEmpty();
            Guard.Argument(logHandler).NotNull();

            this.schedulerId = schedulerId;
            this.jobDataProviders = jobDataProviders;
            this.logHandler = logHandler;
        }

    private bool IsSchedulerActive
        => scheduler is object
           && scheduler.IsStarted
           && !scheduler.IsShutdown;

    internal async Task ScheduleJobsAsync(CancellationToken cancellationToken = default)
    {
            try
            {
                await StartSchedulerAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logHandler.Error("PollingJob ERROR starting scheduler", ex, new { SchedulerId = schedulerId });
                return;
            }

            foreach (var jobDataProvider in jobDataProviders)
            {
                if (!jobDataProvider.PollingDefinition.Enabled)
                {
                    logHandler.Warning(
                        "PollingJob Scheduler not enabled",
                        new
                        {
                            SchedulerId = schedulerId,
                            PollingJobType = jobDataProvider.PollingDefinition.PollingJobType.ToString(),
                            CronExpression = jobDataProvider.PollingDefinition.CronExpression
                        });

                    continue;
                }

                await ScheduleJobAsync(jobDataProvider, cancellationToken).ConfigureAwait(false);
            }
        }

    internal async Task UnscheduleJobsAsync(CancellationToken cancellationToken = default)
    {
            foreach (var jobDataProvider in jobDataProviders)
            {
                if (!jobDataProvider.PollingDefinition.Enabled)
                {
                    continue;
                }

                var trigger = jobDataProvider.Trigger;

                logHandler.Info(
                    "PollingJob unscheduler started",
                    new
                    {
                        SchedulerId = schedulerId,
                        PollingJobType = jobDataProvider.PollingDefinition.PollingJobType.ToString(),
                        TriggerKey = trigger.Key.ToString()
                    });

                var unscheduledJob = await scheduler.UnscheduleJob(trigger.Key).ConfigureAwait(false);

                logHandler.Info("PollingJob unscheduler finished",
                    new
                    {
                        SchedulerId = schedulerId,
                        PollingJobType = jobDataProvider.PollingDefinition.PollingJobType.ToString(),
                        TriggerKey = trigger.Key.ToString(),
                        UnscheduledJob = unscheduledJob.ToString()
                    });
            }
        }

    private async Task ScheduleJobAsync(IJobDataProvider jobDataProvider, CancellationToken cancellationToken)
    {
            try
            {
                var job = jobDataProvider.JobDetail;
                var trigger = jobDataProvider.Trigger;

                var scheduledJob = await scheduler.ScheduleJob(job, trigger, cancellationToken).ConfigureAwait(false);

                logHandler.Info(
                    "PollingJob Scheduler scheduled",
                    new
                    {
                        SchedulerId = schedulerId,
                        PollingJobType = jobDataProvider.PollingDefinition.PollingJobType.ToString(),
                        CronExpression = jobDataProvider.PollingDefinition.CronExpression,
                        ScheduleJob = scheduledJob.ToString()
                    });
            }
            catch (Exception ex)
            {
                logHandler.Error(
                    "PollingJob Scheduler ERROR scheduling",
                    ex,
                    new
                    {
                        SchedulerId = schedulerId,
                        PollingJobType = jobDataProvider.PollingDefinition.PollingJobType.ToString(),
                        CronExpression = jobDataProvider.PollingDefinition.CronExpression
                    });
            }
        }

    private async Task StartSchedulerAsync(CancellationToken cancellationToken)
    {
            lock (internalLock)
            {
                Guard.Argument(scheduler).Null(s => "Scheduler was already started. Please call this method just once.");

                StdSchedulerFactory fact = new StdSchedulerFactory();
                fact.Initialize(new NameValueCollection { { "quartz.scheduler.instanceName", schedulerId } });
                scheduler = fact.GetScheduler(cancellationToken).GetAwaiter().GetResult();

                logHandler.Info("PollingJob Scheduler acquired", new { SchedulerId = schedulerId });
            }

            if (!IsSchedulerActive)
            {
                await scheduler.Start(cancellationToken).ConfigureAwait(false);

                logHandler.Info("PollingJob Scheduler started", new { SchedulerId = schedulerId });
            }
        }
}