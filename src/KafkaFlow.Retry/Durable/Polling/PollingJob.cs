namespace KafkaFlow.Retry.Durable.Polling
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Confluent.Kafka;
    using Dawn;
    using KafkaFlow.Consumers;
    using KafkaFlow.Producers;
    using KafkaFlow.Retry.Durable.Common;
    using KafkaFlow.Retry.Durable.Repository;
    using KafkaFlow.Retry.Durable.Repository.Actions.Read;
    using KafkaFlow.Retry.Durable.Repository.Actions.Update;
    using KafkaFlow.Retry.Durable.Repository.Adapters;
    using KafkaFlow.Retry.Durable.Repository.Model;
    using Quartz;

    [Quartz.DisallowConcurrentExecutionAttribute()]
    internal class PollingJob : IJob
    {
        private const int DefaultPartitionElection = 0;
        private const string LogFacility = "POLLING JOB";
        private static readonly HeadersAdapter headersAdapter = new HeadersAdapter();
        private static readonly InRetryMessageAdapter inRetryMessageAdapter = new InRetryMessageAdapter();
        private TimeSpan expirationInterval = TimeSpan.Zero;

        public async Task Execute(IJobExecutionContext context)
        {
            var jobDataMap = context.JobDetail.JobDataMap;

            Guard.Argument(jobDataMap.ContainsKey(PollingConstants.KafkaRetryDurableQueueRepository), PollingConstants.KafkaRetryDurableQueueRepository)
                 .True("Argument RetryQueueStorage wasn't found and is required for this job");

            Guard.Argument(jobDataMap.ContainsKey(PollingConstants.RetryProducerKey), PollingConstants.RetryProducerKey)
                .True("Argument RetryProducer wasn't found and is required for this job");

            Guard.Argument(jobDataMap.ContainsKey(PollingConstants.PollingConfigKey), PollingConstants.PollingConfigKey)
                .True("Argument PollingConfig wasn't found and is required for this job");

            Guard.Argument(jobDataMap.ContainsKey(PollingConstants.RetryDurableConsumer), PollingConstants.RetryDurableConsumer)
                .True("Argument PollingConfig wasn't found and is required for this job");

            var kafkaRetryDurableQueueRepository = jobDataMap[PollingConstants.KafkaRetryDurableQueueRepository] as IKafkaRetryDurableQueueRepository;
            var retryQueueProducer = jobDataMap[PollingConstants.RetryProducerKey] as IMessageProducer;
            var pollingConfig = jobDataMap[PollingConstants.PollingConfigKey] as KafkaRetryDurableDefinition;
            var retryDurableConsumer = jobDataMap[PollingConstants.RetryDurableConsumer] as IMessageConsumer;

            Guard.Argument(kafkaRetryDurableQueueRepository).NotNull();
            Guard.Argument(retryQueueProducer).NotNull();
            Guard.Argument(pollingConfig).NotNull();

            try
            {
                //policyBuilder.OnLog(new LogMessage(policyBuilder.GetSearchGroupKey(), KafkaRetryLogLevel.Info, LogFacility, "The 'Execute' has been called."));
                if (IsConsumerSelectedForPolling(retryDurableConsumer.Assignment))
                {
                    await this
                        .ProduceInRetryMessagesAsync(
                            kafkaRetryDurableQueueRepository,
                            retryQueueProducer,
                            pollingConfig)
                        .ConfigureAwait(false);
                }
            }
            catch (KafkaRetryException ex)
            {
                //policyBuilder.OnPollingExceptionHandler(ex);
            }
            catch (Exception ex) // dispatch the handler and finish the job with success
            {
                //policyBuilder.OnPollingExceptionHandler(new KafkaRetryException(new RetryError(RetryErrorCode.Polling_UnknownException), ex.Message, ex));
            }
        }

        private TimeSpan GetExpirationInterval(KafkaRetryDurablePollingDefinition kafkaRetryDurablePollingDefinition)
        {
            if (this.expirationInterval != TimeSpan.Zero)
            {
                return this.expirationInterval;
            }

            Guard.Argument(CronExpression.IsValidExpression(kafkaRetryDurablePollingDefinition.CronExpression), nameof(kafkaRetryDurablePollingDefinition.CronExpression)).True();

            var cron = new CronExpression(kafkaRetryDurablePollingDefinition.CronExpression);
            var referenceDate = DateTimeOffset.UtcNow;

            var nextFire = cron.GetNextValidTimeAfter(referenceDate);

            Guard.Argument(nextFire.HasValue, nameof(nextFire)).True();

            var afterNextFire = cron.GetNextValidTimeAfter(nextFire.Value);

            Guard.Argument(afterNextFire.HasValue, nameof(afterNextFire)).True();

            var pollingInterval = afterNextFire.Value - nextFire.Value;

            for (var i = 0; i < kafkaRetryDurablePollingDefinition.ExpirationIntervalFactor; i++)
            {
                this.expirationInterval += pollingInterval;
            }

            return this.expirationInterval;
        }

        private Type GetMessageTypeFromMessageHeaders(IList<MessageHeader> headers)
        {
            var header = headers.First(h => string.Equals(h.Key, KafkaRetryDurableConstants.MessageType));
            return Type.GetType(header.Value.ByteArrayToString());
        }

        private bool IsAbleToBeProduced(RetryQueueItem item, KafkaRetryDurablePollingDefinition kafkaRetryDurablePollingDefinition)
        {
            return item.Status == RetryQueueItemStatus.Waiting
                 || (item.ModifiedStatusDate.HasValue
                    && item.Status == RetryQueueItemStatus.InRetry
                    && DateTime.UtcNow > item.ModifiedStatusDate + this.GetExpirationInterval(kafkaRetryDurablePollingDefinition));
        }

        private bool IsConsumerSelectedForPolling(IEnumerable<TopicPartition> topicPartitions)
            => topicPartitions is object && topicPartitions.Any(tp => tp.Partition == DefaultPartitionElection);

        private async Task ProduceInRetryMessagesAsync(
                    IKafkaRetryDurableQueueRepository queueStorage,
                    IMessageProducer messageProducer,
                    //RetryPolicyBuilder<TKey, TResult> policyBuilder,
                    //NonBlockRetryPolicyConfig policyConfig,
                    KafkaRetryDurableDefinition kafkaRetryDurableDefinition)
        {
            var queueItemsInput =
                new GetQueuesInput(
                    RetryQueueStatus.Active,
                    new RetryQueueItemStatus[] { RetryQueueItemStatus.Waiting },
                    GetQueuesSortOption.ByLastExecution_Ascending,
                    kafkaRetryDurableDefinition.KafkaRetryDurablePollingDefinition.FetchSize,
                    new StuckStatusFilter(
                        RetryQueueItemStatus.InRetry,
                        this.GetExpirationInterval(kafkaRetryDurableDefinition.KafkaRetryDurablePollingDefinition)
                    )
                )
                {
                    SearchGroupKey = "print-console-handler-test"
                };

            var activeQueues = await queueStorage.GetRetryQueuesAsync(queueItemsInput).ConfigureAwait(false);

            if (activeQueues is object && activeQueues.Any())
            {
                foreach (var queue in activeQueues)
                {
                    if (!queue.Items.Any())
                    {
                        continue;
                    }

                    foreach (var item in queue.Items.OrderBy(i => i.Sort))
                    {
                        //policyBuilder.OnLog(new LogMessage(policyBuilder.GetSearchGroupKey(), KafkaRetryLogLevel.Info, LogFacility,
                        //$"An retry item message (QueueId: {queue.Id} ItemId: {item.Id}) from queue group key '{queue.QueueGroupKey}' WILL BE PRODUCED."));

                        if (this.IsAbleToBeProduced(item, kafkaRetryDurableDefinition.KafkaRetryDurablePollingDefinition))
                        {
                            var inputInRetry = new UpdateItemStatusInput(item.Id, RetryQueueItemStatus.InRetry);
                            await queueStorage.UpdateItemAsync(inputInRetry).ConfigureAwait(false);

                            try
                            {
                                await messageProducer.ProduceAsync(
                                        item.Message.Key.ByteArrayToString(),
                                        item.Message.Value.DeserializeObject(this.GetMessageTypeFromMessageHeaders(item.Message.Headers), true),
                                        headersAdapter.AdaptToConfluentHeaders(queue.Id, item)
                                    ).ConfigureAwait(false);
                            }
                            catch (Exception)
                            {
                                var inputWaiting = new UpdateItemStatusInput(item.Id, RetryQueueItemStatus.Waiting);
                                await queueStorage.UpdateItemAsync(inputWaiting).ConfigureAwait(false);

                                throw;
                            }
                        }
                    }
                }
            }
        }
    }
}