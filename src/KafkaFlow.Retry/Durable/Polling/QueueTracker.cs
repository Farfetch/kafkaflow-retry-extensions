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
    private static readonly object s_internalLock = new();
    private readonly IEnumerable<IJobDataProvider> _jobDataProviders;
    private readonly ILogHandler _logHandler;
    private readonly string _schedulerId;
    private IScheduler _scheduler;

    public QueueTracker(
        string schedulerId,
        IEnumerable<IJobDataProvider> jobDataProviders,
        ILogHandler logHandler
    )
    {
        Guard.Argument(schedulerId, nameof(schedulerId)).NotNull().NotEmpty();
        Guard.Argument(jobDataProviders).NotNull().NotEmpty();
        Guard.Argument(logHandler).NotNull();

        _schedulerId = schedulerId;
        _jobDataProviders = jobDataProviders;
        _logHandler = logHandler;
    }

    private bool _isSchedulerActive
        => _scheduler is object
           && _scheduler.IsStarted
           && !_scheduler.IsShutdown;

    internal async Task ScheduleJobsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await StartSchedulerAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logHandler.Error("PollingJob ERROR starting scheduler", ex, new { SchedulerId = _schedulerId });
            return;
        }

        foreach (var jobDataProvider in _jobDataProviders)
        {
            if (!jobDataProvider.PollingDefinition.Enabled)
            {
                _logHandler.Warning(
                    "PollingJob Scheduler not enabled",
                    new
                    {
                        SchedulerId = _schedulerId,
                        PollingJobType = jobDataProvider.PollingDefinition.PollingJobType.ToString(),
                        jobDataProvider.PollingDefinition.CronExpression
                    });

                continue;
            }

            await ScheduleJobAsync(jobDataProvider, cancellationToken).ConfigureAwait(false);
        }
    }

    internal async Task UnscheduleJobsAsync(CancellationToken cancellationToken = default)
    {
        foreach (var jobDataProvider in _jobDataProviders)
        {
            if (!jobDataProvider.PollingDefinition.Enabled)
            {
                continue;
            }

            var trigger = jobDataProvider.Trigger;

            _logHandler.Info(
                "PollingJob unscheduler started",
                new
                {
                    SchedulerId = _schedulerId,
                    PollingJobType = jobDataProvider.PollingDefinition.PollingJobType.ToString(),
                    TriggerKey = trigger.Key.ToString()
                });

            var unscheduledJob = await _scheduler.UnscheduleJob(trigger.Key).ConfigureAwait(false);

            _logHandler.Info("PollingJob unscheduler finished",
                new
                {
                    SchedulerId = _schedulerId,
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

            var scheduledJob = await _scheduler.ScheduleJob(job, trigger, cancellationToken).ConfigureAwait(false);

            _logHandler.Info(
                "PollingJob Scheduler scheduled",
                new
                {
                    SchedulerId = _schedulerId,
                    PollingJobType = jobDataProvider.PollingDefinition.PollingJobType.ToString(),
                    jobDataProvider.PollingDefinition.CronExpression,
                    ScheduleJob = scheduledJob.ToString()
                });
        }
        catch (Exception ex)
        {
            _logHandler.Error(
                "PollingJob Scheduler ERROR scheduling",
                ex,
                new
                {
                    SchedulerId = _schedulerId,
                    PollingJobType = jobDataProvider.PollingDefinition.PollingJobType.ToString(),
                    jobDataProvider.PollingDefinition.CronExpression
                });
        }
    }

    private async Task StartSchedulerAsync(CancellationToken cancellationToken)
    {
        lock (s_internalLock)
        {
            Guard.Argument(_scheduler).Null(s => "Scheduler was already started. Please call this method just once.");

            var fact = new StdSchedulerFactory();
            fact.Initialize(new NameValueCollection { { "quartz.scheduler.instanceName", _schedulerId } });
            _scheduler = fact.GetScheduler(cancellationToken).GetAwaiter().GetResult();

            _logHandler.Info("PollingJob Scheduler acquired", new { SchedulerId = _schedulerId });
        }

        if (!_isSchedulerActive)
        {
            await _scheduler.Start(cancellationToken).ConfigureAwait(false);

            _logHandler.Info("PollingJob Scheduler started", new { SchedulerId = _schedulerId });
        }
    }
}