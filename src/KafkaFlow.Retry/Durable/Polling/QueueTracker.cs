namespace KafkaFlow.Retry.Durable.Polling
{
    using System;
    using System.Threading;
    using Dawn;
    using KafkaFlow.Retry.Durable.Definitions;
    using Quartz;
    using Quartz.Impl;

    internal class QueueTracker
    {
        private static readonly object internalLock = new object();
        private readonly IJobDetail job;
        private readonly ILogHandler logHandler;
        private readonly RetryDurablePollingDefinition retryDurablePollingDefinition;
        private readonly ITrigger trigger;
        private IScheduler scheduler;

        public QueueTracker(
            ILogHandler logHandler,
            RetryDurablePollingDefinition retryDurablePollingDefinition,
            IJobDetailProvider jobDetailProvider,
            ITriggerProvider triggerProvider
        )
        {
            Guard.Argument(logHandler).NotNull();
            Guard.Argument(retryDurablePollingDefinition).NotNull();
            Guard.Argument(jobDetailProvider).NotNull();
            Guard.Argument(triggerProvider).NotNull();

            this.logHandler = logHandler;
            this.retryDurablePollingDefinition = retryDurablePollingDefinition;

            this.job = jobDetailProvider.GetQueuePollingJobDetail();
            this.trigger = triggerProvider.GetQueuePollingTrigger();
        }

        private bool IsSchedulerActive
            => this.scheduler is object
            && this.scheduler.IsStarted
            && !this.scheduler.IsShutdown;

        internal void ScheduleJob(CancellationToken cancellationToken = default)
        {
            try
            {
                Guard.Argument(this.scheduler).Null(s => "Scheduler was already started. Please call this method just once.");

                lock (internalLock)
                {
                    this.scheduler = StdSchedulerFactory.GetDefaultScheduler(cancellationToken).GetAwaiter().GetResult();

                    this.logHandler.Info(
                        "PollingJob Scheduler Acquired",
                        new
                        {
                            PollingId = this.retryDurablePollingDefinition.Id,
                            CronExpression = this.retryDurablePollingDefinition.CronExpression
                        });
                }

                if (!this.IsSchedulerActive)
                {
                    this.scheduler
                        .Start(cancellationToken)
                        .ConfigureAwait(false)
                        .GetAwaiter()
                        .GetResult();

                    this.logHandler.Info(
                        "PollingJob Scheduler Started",
                        new
                        {
                            PollingId = this.retryDurablePollingDefinition.Id,
                            CronExpression = this.retryDurablePollingDefinition.CronExpression
                        });
                }

                var triggerAlreadyDefined = this.scheduler
                    .GetTrigger(this.trigger.Key)
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult();

                if (triggerAlreadyDefined is object)
                {
                    this.logHandler.Info(
                        "PollingJob Scheduler Already Started",
                        new
                        {
                            PollingId = this.retryDurablePollingDefinition.Id,
                            CronExpression = this.retryDurablePollingDefinition.CronExpression
                        });

                    return;
                }

                var scheduledJob = this.scheduler
                    .ScheduleJob(this.job, this.trigger, cancellationToken)
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult();

                this.logHandler.Info(
                    "PollingJob Scheduler Scheduled",
                    new
                    {
                        PollingId = this.retryDurablePollingDefinition.Id,
                        CronExpression = this.retryDurablePollingDefinition.CronExpression,
                        ScheduleJob = scheduledJob.ToString()
                    });
            }
            catch (Exception ex)
            {
                this.logHandler.Error(
                    "PollingJob Scheduler Error",
                    ex,
                    new
                    {
                        PollingId = this.retryDurablePollingDefinition.Id,
                        CronExpression = this.retryDurablePollingDefinition.CronExpression
                    });
            }
        }

        internal void UnscheduleJob(CancellationToken cancellationToken = default)
        {
            this.logHandler.Info(
                "PollingJob Unscheduler Started",
                new
                {
                    TriggerKey = this.trigger.Key.ToString()
                });

            var unscheduledJob = this.scheduler
                .UnscheduleJob(this.trigger.Key)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();

            this.logHandler.Info("PollingJob Unscheduler Finished",
                new
                {
                    UnscheduleJob = unscheduledJob,
                    TriggerKey = this.trigger.Key.ToString()
                });
        }
    }
}