namespace KafkaFlow.Retry.Durable.Polling
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Dawn;
    using KafkaFlow.Consumers;
    using KafkaFlow.Producers;
    using KafkaFlow.Retry.Durable.Repository;
    using Quartz;
    using Quartz.Impl;

    internal class QueueTracker : IDisposable
    {
        private static object internalLock = new object();

        //private readonly KafkaRetryDurablePollingDefinition kafkaRetryDurablePollingDefinition;

        private readonly IMessageConsumer messageConsumer;

        //private readonly RetryPolicyBuilder<TKey, TResult> policyBuilder;
        //private readonly NonBlockRetryPolicyConfig policyConfig;
        private readonly IMessageProducer messageProducer;

        private readonly IKafkaRetryDurableQueueRepository queueStorage;
        private JobKey jobKey;
        private IScheduler scheduler;

        public QueueTracker(
            IKafkaRetryDurableQueueRepository queueStorage,
            IMessageProducer messageProducer,
            IMessageConsumer messageConsumer
        )
        {
            this.queueStorage = queueStorage;
            this.messageProducer = messageProducer;
            this.messageConsumer = messageConsumer;
        }

        private bool IsSchedulerActive => this.scheduler is object && this.scheduler.IsStarted && !this.scheduler.IsShutdown;

        public void Dispose()
        {
        }

        internal void PauseJob(CancellationToken cancellationToken)
        {
            lock (internalLock)
            {
                if (this.IsSchedulerActive)
                {
                    this.scheduler.PauseJob(this.jobKey, cancellationToken).GetAwaiter().GetResult();

                    //this.policyBuilder.OnLog(new LogMessage(this.policyBuilder.GetSearchGroupKey(), KafkaRetryLogLevel.Info, "QUEUE TRACKER", "The 'PauseJob' has been called."));
                }
            }
        }

        internal void ResumeJob(CancellationToken cancellationToken)
        {
            lock (internalLock)
            {
                if (this.IsSchedulerActive)
                {
                    this.scheduler.ResumeJob(this.jobKey, cancellationToken).GetAwaiter().GetResult();

                    //this.policyBuilder.OnLog(new LogMessage(this.policyBuilder.GetSearchGroupKey(), KafkaRetryLogLevel.Info, "QUEUE TRACKER", "The 'ResumeJob' has been called."));
                }
            }
        }

        internal Task ResumeJobAsync(CancellationToken cancellationToken = default(CancellationToken))
            => this.scheduler.ResumeJob(this.jobKey, cancellationToken);

        internal async Task ScheduleJobAsync(
            KafkaRetryDurableDefinition kafkaRetryDurableDefinition,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Guard.Argument(kafkaRetryDurableDefinition).NotNull();
            Guard.Argument(CronExpression.IsValidExpression(kafkaRetryDurableDefinition.KafkaRetryDurablePollingDefinition.CronExpression), nameof(kafkaRetryDurableDefinition.KafkaRetryDurablePollingDefinition.CronExpression)).True();
            Guard.Argument(this.scheduler).Null(s => "Scheduler was already started. Please call this method just once.");

            //this.policyBuilder.OnLog(new LogMessage(this.policyBuilder.GetSearchGroupKey(), KafkaRetryLogLevel.Info, "QUEUE TRACKER", "The 'ScheduleJobAsync' has been called."));

            lock (internalLock)
            {
                this.scheduler = StdSchedulerFactory.GetDefaultScheduler(cancellationToken).GetAwaiter().GetResult();
            }

            await this.scheduler.Start(cancellationToken);

            JobDataMap dataMap = new JobDataMap();

            dataMap.Add(PollingConstants.KafkaRetryDurableQueueRepository, this.queueStorage);
            dataMap.Add(PollingConstants.RetryProducerKey, this.messageProducer);
            dataMap.Add(PollingConstants.PollingConfigKey, kafkaRetryDurableDefinition);
            dataMap.Add(PollingConstants.RetryDurableConsumer, this.messageConsumer);

            IJobDetail job = JobBuilder
                .Create<PollingJob>()
                .SetJobData(dataMap)
                .Build();

            this.jobKey = job.Key;

            ITrigger trigger = TriggerBuilder
                .Create()
                .WithIdentity($"pollingJob_{/*this.policyBuilder.Config.MainTopicName*/ string.Empty}", "queueTrackerGroup")
                .WithCronSchedule(kafkaRetryDurableDefinition.KafkaRetryDurablePollingDefinition.CronExpression)
                .StartNow()
                .WithPriority(1)
                .Build();

            await this.scheduler.ScheduleJob(job, trigger, cancellationToken).ConfigureAwait(false);
        }

        internal Task ShutdownAsync(CancellationToken cancellationToken)
        {
            lock (internalLock)
            {
                if (this.IsSchedulerActive)
                {
                    //this.policyBuilder.OnLog(new LogMessage(this.policyBuilder.GetSearchGroupKey(), KafkaRetryLogLevel.Info, "QUEUE TRACKER", "The 'ShutdownAsync' has been called."));

                    return this.scheduler.Shutdown(cancellationToken);
                }
            }

            return Task.CompletedTask;
        }
    }
}