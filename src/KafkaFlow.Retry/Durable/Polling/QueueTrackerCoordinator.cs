namespace KafkaFlow.Retry.Durable.Polling
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Confluent.Kafka;
    using Dawn;
    using KafkaFlow.Consumers;

    internal class QueueTrackerCoordinator : IQueueTrackerCoordinator
    {
        private const int DefaultPartitionElection = 0;
        private const string LogFacility = "QUEUE TRACKER COORDINATOR";

        private readonly IConsumerAccessor consumerAccessor;
        private readonly IQueueTrackerFactory queueTrackerFactory;
        private CancellationToken cancellationToken;

        private QueueTracker queueTracker;

        private KafkaRetryDurableDefinition queueTrackerConfig;

        public QueueTrackerCoordinator(
            IQueueTrackerFactory queueTrackerFactory,
            IConsumerAccessor consumerAccessor)
        {
            this.queueTrackerFactory = queueTrackerFactory;
            this.consumerAccessor = consumerAccessor;
        }

        public void Dispose()
        {
            if (this.queueTracker is object)
            {
                this.queueTracker.Dispose();
                this.queueTracker = null;
            }
        }

        public async Task InitializeAsync(
            KafkaRetryDurableDefinition kafkaRetryDurableDefinition,
            CancellationToken cancellationToken
        )
        {
            Guard.Argument(kafkaRetryDurableDefinition).NotNull();
            Guard.Argument(this.queueTrackerConfig).Null(p => "Queue tracker coordinator has already initialized");

            this.queueTrackerConfig = kafkaRetryDurableDefinition;
            this.cancellationToken = cancellationToken;

            if (!kafkaRetryDurableDefinition.KafkaRetryDurablePollingDefinition.Enabled)
            {
                return;
            }

            //this.policyBuilder.OnLog(new Retry.LogMessage(this.policyBuilder.GetSearchGroupKey(), KafkaRetryLogLevel.Info, LogFacility,$"The Retry Consumer is going to start for '{this.policyBuilder.GetSearchGroupKey()}'"));

            //this.retryConsumer.Start(action, cancellationToken);

            //this.policyBuilder.SetInternalPartitionsAssignedHandler(OnPartitionsAssigned);
            //this.policyBuilder.SetInternalPartitionsRevokedHandler(OnPartitionsRevoked);

            //if (this.IsConsumerSelectedForPolling(this.consumerAccessor.GetConsumer("test").Assignment))
            //{
            await this.CreateAndScheduleAsync(kafkaRetryDurableDefinition, cancellationToken).ConfigureAwait(false);
            //}
        }

        public async Task ShutdownAsync(CancellationToken cancellationToken)
        {
            if (this.queueTracker is object)
            {
                await this.queueTracker.ShutdownAsync(cancellationToken);

                //this.policyBuilder.OnLog(new Retry.LogMessage(this.policyBuilder.GetSearchGroupKey(), KafkaRetryLogLevel.Info, LogFacility,$"The Queue Tracker has been shutdown for '{this.policyBuilder.GetSearchGroupKey()}'"));
            }

            //if (this.retryConsumer is object)
            //{
            //    this.retryConsumer.Shutdown();

            //this.policyBuilder.OnLog(new Retry.LogMessage(this.policyBuilder.GetSearchGroupKey(), KafkaRetryLogLevel.Info, LogFacility,$"The Retry Consumer been shutdown for '{this.policyBuilder.GetSearchGroupKey()}'"));
            //}
        }

        private Task CreateAndScheduleAsync(KafkaRetryDurableDefinition kafkaRetryDurableDefinition, CancellationToken cancellationToken)
        {
            this.queueTracker = this.queueTrackerFactory.Create();

            return this.queueTracker.ScheduleJobAsync(kafkaRetryDurableDefinition, cancellationToken);
        }

        private bool IsConsumerSelectedForPolling(IEnumerable<TopicPartition> topicPartitions)
            => topicPartitions is object && topicPartitions.Any(tp => tp.Partition == DefaultPartitionElection);

        //private void OnPartitionsAssigned(IConsumer<TKey, byte[]> consumer, IEnumerable<TopicPartition> topicPartitions)
        //{
        //    if (this.IsConsumerSelectedForPolling(topicPartitions))
        //    {
        //        if (this.queueTracker is null) // create
        //        {
        //            this.CreateAndScheduleAsync(this.queueTrackerConfig, this.cancellationToken).GetAwaiter().GetResult();
        //        }
        //        else // resume
        //        {
        //            this.queueTracker.ResumeJob(this.cancellationToken);
        //        }
        //    }
        //    else // if was previously selected we need to stand by
        //    {
        //        this.queueTracker?.PauseJob(this.cancellationToken);
        //    }
        //}

        //private void OnPartitionsRevoked(IConsumer<TKey, byte[]> consumer, IEnumerable<TopicPartitionOffset> topicPartitionOffsets)
        //{
        //    this.queueTracker?.PauseJob(this.cancellationToken);
        //}
    }
}