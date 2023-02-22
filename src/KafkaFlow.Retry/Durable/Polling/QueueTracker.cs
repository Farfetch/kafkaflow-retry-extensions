namespace KafkaFlow.Retry.Durable.Polling
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Threading;
    using Dawn;
    using Quartz;
    using Quartz.Impl;

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
            => this.scheduler is object
            && this.scheduler.IsStarted
            && !this.scheduler.IsShutdown;

        internal void ScheduleJobs(CancellationToken cancellationToken = default)
        {
            try
            {
                this.StartScheduler(cancellationToken);
            }
            catch (Exception ex)
            {
                this.logHandler.Error("PollingJob ERROR starting scheduler", ex, new { SchedulerId = this.schedulerId });
                return;
            }

            foreach (var jobDataProvider in this.jobDataProviders)
            {
                if (!jobDataProvider.PollingDefinition.Enabled)
                {
                    this.logHandler.Warning(
                        "PollingJob Scheduler not enabled",
                        new
                        {
                            SchedulerId = this.schedulerId,
                            PollingJobType = jobDataProvider.PollingDefinition.PollingJobType.ToString(),
                            CronExpression = jobDataProvider.PollingDefinition.CronExpression
                        });

                    continue;
                }

                this.ScheduleJob(jobDataProvider, cancellationToken);
            }
        }

        internal void UnscheduleJobs(CancellationToken cancellationToken = default)
        {
            foreach (var jobDataProvider in this.jobDataProviders)
            {
                if (!jobDataProvider.PollingDefinition.Enabled)
                {
                    continue;
                }

                var trigger = jobDataProvider.Trigger;

                this.logHandler.Info(
                    "PollingJob unscheduler started",
                    new
                    {
                        SchedulerId = this.schedulerId,
                        PollingJobType = jobDataProvider.PollingDefinition.PollingJobType.ToString(),
                        TriggerKey = trigger.Key.ToString()
                    });

                var unscheduledJob = this.scheduler
                    .UnscheduleJob(trigger.Key)
                    .GetAwaiter()
                    .GetResult();

                this.logHandler.Info("PollingJob unscheduler finished",
                    new
                    {
                        SchedulerId = this.schedulerId,
                        PollingJobType = jobDataProvider.PollingDefinition.PollingJobType.ToString(),
                        TriggerKey = trigger.Key.ToString(),
                        UnscheduledJob = unscheduledJob.ToString()
                    });
            }
        }

        private void ScheduleJob(IJobDataProvider jobDataProvider, CancellationToken cancellationToken)
        {
            try
            {
                var job = jobDataProvider.GetPollingJobDetail();
                var trigger = jobDataProvider.Trigger;

                var scheduledJob = this.scheduler
                    .ScheduleJob(job, trigger, cancellationToken)
                    .GetAwaiter()
                    .GetResult();

                this.logHandler.Info(
                    "PollingJob Scheduler scheduled",
                    new
                    {
                        SchedulerId = this.schedulerId,
                        PollingJobType = jobDataProvider.PollingDefinition.PollingJobType.ToString(),
                        CronExpression = jobDataProvider.PollingDefinition.CronExpression,
                        ScheduleJob = scheduledJob.ToString()
                    });
            }
            catch (Exception ex)
            {
                this.logHandler.Error(
                    "PollingJob Scheduler ERROR scheduling",
                    ex,
                    new
                    {
                        SchedulerId = this.schedulerId,
                        PollingJobType = jobDataProvider.PollingDefinition.PollingJobType.ToString(),
                        CronExpression = jobDataProvider.PollingDefinition.CronExpression
                    });
            }
        }

        private void StartScheduler(CancellationToken cancellationToken)
        {
            lock (internalLock)
            {
                Guard.Argument(this.scheduler).Null(s => "Scheduler was already started. Please call this method just once.");

                StdSchedulerFactory fact = new StdSchedulerFactory();
                fact.Initialize(new NameValueCollection { { "quartz.scheduler.instanceName", this.schedulerId } });
                this.scheduler = fact.GetScheduler(cancellationToken).GetAwaiter().GetResult();

                this.logHandler.Info("PollingJob Scheduler acquired", new { SchedulerId = this.schedulerId });
            }

            if (!this.IsSchedulerActive)
            {
                this.scheduler
                    .Start(cancellationToken)
                    .GetAwaiter()
                    .GetResult();

                this.logHandler.Info("PollingJob Scheduler started", new { SchedulerId = this.schedulerId });
            }
        }
    }
}