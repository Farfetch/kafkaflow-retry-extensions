﻿namespace KafkaFlow.Retry.Durable.Polling
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Dawn;
    using KafkaFlow.Retry.Durable.Definitions;
    using KafkaFlow.Retry.Durable.Encoders;
    using KafkaFlow.Retry.Durable.Repository;
    using KafkaFlow.Retry.Durable.Repository.Actions.Read;
    using KafkaFlow.Retry.Durable.Repository.Actions.Update;
    using KafkaFlow.Retry.Durable.Repository.Adapters;
    using KafkaFlow.Retry.Durable.Repository.Model;
    using Quartz;

    [Quartz.DisallowConcurrentExecutionAttribute()]
    internal class QueuePollingJob : IJob
    {
        private TimeSpan expirationInterval = TimeSpan.Zero;

        public async Task Execute(IJobExecutionContext context)
        {
            var jobDataMap = context.JobDetail.JobDataMap;

            Guard.Argument(jobDataMap.ContainsKey(QueuePollingJobConstants.RetryDurableQueueRepository), QueuePollingJobConstants.RetryDurableQueueRepository)
                .True("Argument RetryDurableQueueRepository wasn't found and is required for this job");

            Guard.Argument(jobDataMap.ContainsKey(QueuePollingJobConstants.RetryDurableMessageProducer), QueuePollingJobConstants.RetryDurableMessageProducer)
                .True("Argument RetryDurableProducer wasn't found and is required for this job");

            Guard.Argument(jobDataMap.ContainsKey(QueuePollingJobConstants.RetryDurablePollingDefinition), QueuePollingJobConstants.RetryDurablePollingDefinition)
                .True("Argument RetryDurablePollingDefinition wasn't found and is required for this job");

            Guard.Argument(jobDataMap.ContainsKey(QueuePollingJobConstants.LogHandler), QueuePollingJobConstants.LogHandler)
                .True("Argument LogHandler wasn't found and is required for this job");

            Guard.Argument(jobDataMap.ContainsKey(QueuePollingJobConstants.MessageHeadersAdapter), QueuePollingJobConstants.MessageHeadersAdapter)
                .True("Argument MessageHeadersAdapter wasn't found and is required for this job");

            Guard.Argument(jobDataMap.ContainsKey(QueuePollingJobConstants.MessageAdapter), QueuePollingJobConstants.MessageAdapter)
                .True("Argument MessageAdapter wasn't found and is required for this job");

            Guard.Argument(jobDataMap.ContainsKey(QueuePollingJobConstants.Utf8Encoder), QueuePollingJobConstants.Utf8Encoder)
                .True("Argument Utf8Encoder wasn't found and is required for this job");

            var retryDurableQueueRepository = jobDataMap[QueuePollingJobConstants.RetryDurableQueueRepository] as IRetryDurableQueueRepository;
            var retryDurableProducer = jobDataMap[QueuePollingJobConstants.RetryDurableMessageProducer] as IMessageProducer;
            var retryDurablePollingDefinition = jobDataMap[QueuePollingJobConstants.RetryDurablePollingDefinition] as RetryDurablePollingDefinition;
            var logHandler = jobDataMap[QueuePollingJobConstants.LogHandler] as ILogHandler;
            var messageHeadersAdapter = jobDataMap[QueuePollingJobConstants.MessageHeadersAdapter] as IMessageHeadersAdapter;
            var messageAdapter = jobDataMap[QueuePollingJobConstants.MessageAdapter] as IMessageAdapter;
            var utf8Encoder = jobDataMap[QueuePollingJobConstants.Utf8Encoder] as IUtf8Encoder;

            Guard.Argument(retryDurableQueueRepository).NotNull();
            Guard.Argument(retryDurableProducer).NotNull();
            Guard.Argument(retryDurablePollingDefinition).NotNull();
            Guard.Argument(logHandler).NotNull();
            Guard.Argument(messageHeadersAdapter).NotNull();
            Guard.Argument(messageAdapter).NotNull();
            Guard.Argument(utf8Encoder).NotNull();

            try
            {
                logHandler.Info(
                    "PollingJob starts execution",
                    new
                    {
                        Name = context.Trigger.Key.Name
                    }
                );

                var queueItemsInput =
                   new GetQueuesInput(
                       RetryQueueStatus.Active,
                       new RetryQueueItemStatus[] { RetryQueueItemStatus.Waiting },
                       GetQueuesSortOption.ByLastExecution_Ascending,
                       retryDurablePollingDefinition.FetchSize,
                       new StuckStatusFilter(
                           RetryQueueItemStatus.InRetry,
                           this.GetExpirationInterval(retryDurablePollingDefinition)
                       )
                   )
                   {
                       SearchGroupKey = retryDurablePollingDefinition.Id
                   };

                var activeQueues = await retryDurableQueueRepository
                    .GetRetryQueuesAsync(queueItemsInput)
                    .ConfigureAwait(false);

                logHandler.Verbose(
                    "PollingJob number of active queues",
                    new
                    {
                        activeQueues
                    }
                );

                foreach (var queue in activeQueues)
                {
                    if (!queue.Items.Any())
                    {
                        logHandler.Verbose(
                            "PollingJob queue with no items",
                            new
                            {
                                QueueId = queue.Id,
                                QueueGroupKey = queue.QueueGroupKey
                            }
                        );

                        continue;
                    }

                    foreach (var item in queue.Items.OrderBy(i => i.Sort))
                    {
                        if (!this.IsAbleToBeProduced(item, retryDurablePollingDefinition))
                        {
                            logHandler.Verbose(
                                "PollingJob queue item is not able to be produced",
                                new
                                {
                                    QueueId = queue.Id,
                                    QueueGroupKey = queue.QueueGroupKey,
                                    ItemId = item.Id,
                                    LastExecution = item.LastExecution,
                                    Status = item.Status
                                }
                            );

                            continue;
                        }

                        await retryDurableQueueRepository
                                .UpdateItemAsync(
                                    new UpdateItemStatusInput(
                                        item.Id,
                                        RetryQueueItemStatus.InRetry))
                                .ConfigureAwait(false);

                        try
                        {
                            await retryDurableProducer
                                .ProduceAsync(
                                    item.Message.Key,
                                    item.Message.Value,
                                    this.GetMessageHeaders(messageHeadersAdapter, utf8Encoder, queue.Id, item)
                                ).ConfigureAwait(false);

                            logHandler.Verbose(
                                "PollingJob queue item produced",
                                new
                                {
                                    QueueId = queue.Id,
                                    QueueGroupKey = queue.QueueGroupKey,
                                    ItemId = item.Id,
                                    LastExecution = item.LastExecution,
                                    Status = item.Status
                                }
                            );
                        }
                        catch (Exception ex)
                        {
                            logHandler.Error(
                                "Exception on queue PollingJob execution producing to retry topic",
                                ex,
                                new
                                {
                                    ItemId = item.Id,
                                    QueueId = queue.Id
                                });

                            await retryDurableQueueRepository
                                .UpdateItemAsync(
                                    new UpdateItemStatusInput(
                                        item.Id,
                                        RetryQueueItemStatus.Waiting))
                                .ConfigureAwait(false);

                            throw;
                        }
                    }
                }
            }
            catch (RetryDurableException rdex)
            {
                logHandler.Error("RetryDurableException on queue PollingJob execution", rdex, null);
            }
            catch (Exception ex)
            {
                logHandler.Error("Exception on queue PollingJob execution", ex, null);
            }
        }

        private TimeSpan GetExpirationInterval(RetryDurablePollingDefinition retryDurablePollingDefinition)
        {
            if (this.expirationInterval != TimeSpan.Zero)
            {
                return this.expirationInterval;
            }

            var cron = new CronExpression(retryDurablePollingDefinition.CronExpression);
            var referenceDate = DateTimeOffset.UtcNow;

            var nextFire = cron.GetNextValidTimeAfter(referenceDate);

            Guard.Argument(nextFire.HasValue, nameof(nextFire)).True();

            var afterNextFire = cron.GetNextValidTimeAfter(nextFire.Value);

            Guard.Argument(afterNextFire.HasValue, nameof(afterNextFire)).True();

            var pollingInterval = afterNextFire.Value - nextFire.Value;

            for (var i = 0; i < retryDurablePollingDefinition.ExpirationIntervalFactor; i++)
            {
                this.expirationInterval += pollingInterval;
            }

            return this.expirationInterval;
        }

        private IMessageHeaders GetMessageHeaders(
            IMessageHeadersAdapter messageHeadersAdapter,
            IUtf8Encoder utf8Encoder,
            Guid queueId,
            RetryQueueItem item)
        {
            var messageHeaders = messageHeadersAdapter.AdaptMessageHeadersFromRepository(item.Message.Headers);

            messageHeaders.Add(RetryDurableConstants.AttemptsCount, utf8Encoder.Encode(item.AttemptsCount.ToString()));
            messageHeaders.Add(RetryDurableConstants.QueueId, utf8Encoder.Encode(queueId.ToString()));
            messageHeaders.Add(RetryDurableConstants.ItemId, utf8Encoder.Encode(item.Id.ToString()));
            messageHeaders.Add(RetryDurableConstants.Sort, utf8Encoder.Encode(item.Sort.ToString()));

            return messageHeaders;
        }

        private bool IsAbleToBeProduced(RetryQueueItem item, RetryDurablePollingDefinition retryDurablePollingDefinition)
        {
            return item.Status == RetryQueueItemStatus.Waiting
                 || (item.ModifiedStatusDate.HasValue
                    && item.Status == RetryQueueItemStatus.InRetry
                    && DateTime.UtcNow > item.ModifiedStatusDate + this.GetExpirationInterval(retryDurablePollingDefinition));
        }
    }
}