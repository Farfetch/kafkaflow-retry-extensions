namespace KafkaFlow.Retry.Durable.Polling
{
    using System.Threading;
    using System.Threading.Tasks;
    using Dawn;
    using KafkaFlow.Producers;
    using KafkaFlow.Retry.Durable.Polling.Strategies;
    using KafkaFlow.Retry.Durable.Repository;
    using Quartz;
    using Quartz.Impl;

    internal class QueueTracker
    {
        private static object internalLock = new object();
        private readonly KafkaRetryDurablePollingDefinition kafkaRetryDurablePollingDefinition;
        private readonly IMessageProducer messageProducer;
        private readonly IPollingJobStrategyProvider pollingJobStrategyProvider;
        private readonly IKafkaRetryDurableQueueRepository queueStorage;
        private readonly bool waitForJobsToComplete = true;
        private IScheduler scheduler;

        public QueueTracker(
            IKafkaRetryDurableQueueRepository queueStorage,
            IMessageProducer messageProducer,
            KafkaRetryDurablePollingDefinition kafkaRetryDurablePollingDefinition,
            IPollingJobStrategyProvider pollingJobStrategyProvider
        )
        {
            Guard.Argument(CronExpression.IsValidExpression(kafkaRetryDurablePollingDefinition.CronExpression), nameof(kafkaRetryDurablePollingDefinition.CronExpression)).True();

            this.queueStorage = queueStorage;
            this.messageProducer = messageProducer;
            this.kafkaRetryDurablePollingDefinition = kafkaRetryDurablePollingDefinition;
            this.pollingJobStrategyProvider = pollingJobStrategyProvider;
        }

        private bool IsSchedulerActive
            => this.scheduler is object && this.scheduler.IsStarted && !this.scheduler.IsShutdown;

        internal async Task ScheduleJobAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            Guard.Argument(this.scheduler).Null(s => "Scheduler was already started. Please call this method just once.");

            lock (internalLock)
            {
                this.scheduler = StdSchedulerFactory.GetDefaultScheduler(cancellationToken).GetAwaiter().GetResult();
            }

            await this.scheduler.Start(cancellationToken);

            JobDataMap dataMap = new JobDataMap();
            dataMap.Add(PollingConstants.KafkaRetryDurableQueueRepository, this.queueStorage);
            dataMap.Add(PollingConstants.KafkaRetryDurableProducer, this.messageProducer);
            dataMap.Add(PollingConstants.KafkaRetryDurablePollingDefinition, this.kafkaRetryDurablePollingDefinition);
            dataMap.Add(PollingConstants.KafkaRetryDurablePollingJobStrategy, this.GetPollingJobStrategyProvider(this.kafkaRetryDurablePollingDefinition));

            IJobDetail job = JobBuilder
                .Create<PollingJob>()
                .SetJobData(dataMap)
                .Build();

            ITrigger trigger = TriggerBuilder
                .Create()
                .WithIdentity($"pollingJob_{this.kafkaRetryDurablePollingDefinition.Id}", "queueTrackerGroup")
                .WithCronSchedule(kafkaRetryDurablePollingDefinition.CronExpression)
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
                    return this.scheduler.Shutdown(waitForJobsToComplete, cancellationToken);
                }
            }

            return Task.CompletedTask;
        }

        private IPollingJobStrategy GetPollingJobStrategyProvider(KafkaRetryDurablePollingDefinition kafkaRetryDurablePollingDefinition)
           => this.pollingJobStrategyProvider.GetPollingJobStrategy(kafkaRetryDurablePollingDefinition.Strategy);
    }
}