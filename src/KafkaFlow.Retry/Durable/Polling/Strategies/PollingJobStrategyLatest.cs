namespace KafkaFlow.Retry.Durable.Polling.Strategies
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Dawn;
    using KafkaFlow.Producers;
    using KafkaFlow.Retry.Durable.Common;
    using KafkaFlow.Retry.Durable.Repository;
    using KafkaFlow.Retry.Durable.Repository.Actions.Read;
    using KafkaFlow.Retry.Durable.Repository.Actions.Update;
    using KafkaFlow.Retry.Durable.Repository.Adapters;
    using KafkaFlow.Retry.Durable.Repository.Model;
    using Quartz;

    internal class PollingJobStrategyLatest : IPollingJobStrategy
    {
        private static readonly HeadersAdapter headersAdapter = new HeadersAdapter();
        private TimeSpan expirationInterval = TimeSpan.Zero;
        public Strategy Strategy => Strategy.Latest;

        public async Task ExecuteAsync(
            IKafkaRetryDurableQueueRepository queueStorage,
            IMessageProducer messageProducer,
            KafkaRetryDurablePollingDefinition kafkaRetryDurablePollingDefinition)
        {
            var queueItemsInput =
                   new GetQueuesInput(
                       RetryQueueStatus.Active,
                       new RetryQueueItemStatus[] { RetryQueueItemStatus.Waiting },
                       GetQueuesSortOption.ByLastExecution_Ascending,
                       kafkaRetryDurablePollingDefinition.FetchSize,
                       new StuckStatusFilter(
                           RetryQueueItemStatus.InRetry,
                           this.GetExpirationInterval(kafkaRetryDurablePollingDefinition)
                       )
                   )
                   {
                       SearchGroupKey = kafkaRetryDurablePollingDefinition.Id
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

                    var queueItemsOrdered = queue.Items.OrderBy(i => i.Sort);
                    var item = queueItemsOrdered.Last();

                    if (this.IsAbleToBeProduced(item, kafkaRetryDurablePollingDefinition))
                    {
                        foreach (var itemToCancel in queueItemsOrdered.Take(queue.Items.Count() - 1))
                        {
                            var inputCancelled = new UpdateItemStatusInput(itemToCancel.Id, RetryQueueItemStatus.Cancelled);
                            await queueStorage.UpdateItemAsync(inputCancelled).ConfigureAwait(false);
                        }

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
    }
}