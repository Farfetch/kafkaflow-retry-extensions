namespace KafkaFlow.Retry.Durable.Polling.Jobs
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Dawn;
    using KafkaFlow.Retry.Durable.Definitions.Polling;
    using KafkaFlow.Retry.Durable.Encoders;
    using KafkaFlow.Retry.Durable.Repository;
    using KafkaFlow.Retry.Durable.Repository.Actions.Read;
    using KafkaFlow.Retry.Durable.Repository.Actions.Update;
    using KafkaFlow.Retry.Durable.Repository.Adapters;
    using KafkaFlow.Retry.Durable.Repository.Model;
    using Quartz;

    [Quartz.DisallowConcurrentExecutionAttribute()]
    internal class RetryDurablePollingJob : IJob
    {
        private TimeSpan expirationInterval = TimeSpan.Zero;

        public async Task Execute(IJobExecutionContext context)
        {
            var jobDataMap = context.JobDetail.JobDataMap;

            Guard.Argument(jobDataMap.ContainsKey(PollingJobConstants.RetryDurableQueueRepository), PollingJobConstants.RetryDurableQueueRepository)
                .True("Argument RetryDurableQueueRepository wasn't found and is required for this job");

            Guard.Argument(jobDataMap.ContainsKey(PollingJobConstants.RetryDurableMessageProducer), PollingJobConstants.RetryDurableMessageProducer)
                .True("Argument RetryDurableProducer wasn't found and is required for this job");

            Guard.Argument(jobDataMap.ContainsKey(PollingJobConstants.RetryDurablePollingDefinition), PollingJobConstants.RetryDurablePollingDefinition)
                .True("Argument RetryDurablePollingDefinition wasn't found and is required for this job");

            Guard.Argument(jobDataMap.ContainsKey(PollingJobConstants.LogHandler), PollingJobConstants.LogHandler)
                .True("Argument LogHandler wasn't found and is required for this job");

            Guard.Argument(jobDataMap.ContainsKey(PollingJobConstants.MessageHeadersAdapter), PollingJobConstants.MessageHeadersAdapter)
                .True("Argument MessageHeadersAdapter wasn't found and is required for this job");

            Guard.Argument(jobDataMap.ContainsKey(PollingJobConstants.MessageAdapter), PollingJobConstants.MessageAdapter)
                .True("Argument MessageAdapter wasn't found and is required for this job");

            Guard.Argument(jobDataMap.ContainsKey(PollingJobConstants.Utf8Encoder), PollingJobConstants.Utf8Encoder)
                .True("Argument Utf8Encoder wasn't found and is required for this job");

            var retryDurableQueueRepository = jobDataMap[PollingJobConstants.RetryDurableQueueRepository] as IRetryDurableQueueRepository;
            var retryDurableProducer = jobDataMap[PollingJobConstants.RetryDurableMessageProducer] as IMessageProducer;
            var retryDurablePollingDefinition = jobDataMap[PollingJobConstants.RetryDurablePollingDefinition] as RetryDurablePollingDefinition;
            var logHandler = jobDataMap[PollingJobConstants.LogHandler] as ILogHandler;
            var messageHeadersAdapter = jobDataMap[PollingJobConstants.MessageHeadersAdapter] as IMessageHeadersAdapter;
            var messageAdapter = jobDataMap[PollingJobConstants.MessageAdapter] as IMessageAdapter;
            var utf8Encoder = jobDataMap[PollingJobConstants.Utf8Encoder] as IUtf8Encoder;
            var schedulerId = jobDataMap[PollingJobConstants.SchedulerId] as string;

            Guard.Argument(retryDurableQueueRepository).NotNull();
            Guard.Argument(retryDurableProducer).NotNull();
            Guard.Argument(retryDurablePollingDefinition).NotNull();
            Guard.Argument(logHandler).NotNull();
            Guard.Argument(messageHeadersAdapter).NotNull();
            Guard.Argument(messageAdapter).NotNull();
            Guard.Argument(utf8Encoder).NotNull();
            Guard.Argument(schedulerId).NotNull().NotEmpty();

            try
            {
                logHandler.Info(
                    $"{nameof(RetryDurablePollingJob)} starts execution",
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
                       SearchGroupKey = schedulerId
                   };

                var activeQueues = await retryDurableQueueRepository
                    .GetRetryQueuesAsync(queueItemsInput)
                    .ConfigureAwait(false);

                logHandler.Verbose(
                    $"{nameof(RetryDurablePollingJob)} number of active queues",
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
                            $"{nameof(RetryDurablePollingJob)} queue with no items",
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
                                $"{nameof(RetryDurablePollingJob)} queue item is not able to be produced",
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
                                $"{nameof(RetryDurablePollingJob)} queue item produced",
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
                                $"Exception on queue {nameof(RetryDurablePollingJob)} execution producing to retry topic",
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
                logHandler.Error($"RetryDurableException on queue {nameof(RetryDurablePollingJob)} execution", rdex, null);
            }
            catch (Exception ex)
            {
                logHandler.Error($"Exception on queue {nameof(RetryDurablePollingJob)} execution", ex, null);
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