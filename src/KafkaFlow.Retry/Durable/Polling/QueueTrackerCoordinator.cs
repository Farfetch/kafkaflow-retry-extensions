namespace KafkaFlow.Retry.Durable.Polling
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Confluent.Kafka;
    using Dawn;

    internal class QueueTrackerCoordinator : IQueueTrackerCoordinator
    {
        private const int DefaultPartitionElection = 0;

        private readonly KafkaRetryDurablePollingDefinition kafkaRetryDurablePollingDefinition;
        private readonly IQueueTrackerFactory queueTrackerFactory;
        private QueueTracker queueTracker;

        public QueueTrackerCoordinator(
            IQueueTrackerFactory queueTrackerFactory,
            KafkaRetryDurablePollingDefinition kafkaRetryDurablePollingDefinition)
        {
            Guard.Argument(this.kafkaRetryDurablePollingDefinition).Null(p => "No polling definitions was found");

            this.queueTrackerFactory = queueTrackerFactory;
            this.kafkaRetryDurablePollingDefinition = kafkaRetryDurablePollingDefinition;
        }
        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            if (!kafkaRetryDurablePollingDefinition.Enabled)
            {
                return;
            }

            this.queueTracker = this.queueTrackerFactory.Create();
            await this.queueTracker.ScheduleJobAsync(cancellationToken).ConfigureAwait(false);

        }

        public async Task ShutdownAsync(CancellationToken cancellationToken)
        {
            if (this.queueTracker is object)
            {
                await this.queueTracker.ShutdownAsync(cancellationToken);
            }
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