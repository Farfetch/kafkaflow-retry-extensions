namespace KafkaFlow.Retry.Durable.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using KafkaFlow.Retry.Durable;
    using KafkaFlow.Retry.Durable.Common;
    using KafkaFlow.Retry.Durable.Repository.Actions.Create;
    using KafkaFlow.Retry.Durable.Repository.Actions.Read;
    using KafkaFlow.Retry.Durable.Repository.Actions.Update;
    using KafkaFlow.Retry.Durable.Repository.Adapters;
    using KafkaFlow.Retry.Durable.Repository.Model;
    using Polly;

    internal class KafkaRetryDurableQueueRepository : IKafkaRetryDurableQueueRepository
    {
        private const int DefaultMaxWaitInSeconds = 60;
        private const int MaxAttempts = 6;
        private readonly IHeadersAdapter headersAdapter;
        private readonly KafkaRetryDurablePollingDefinition kafkaRetryDurablePollingDefinition;
        private readonly IKafkaRetryDurableQueueRepositoryProvider retryQueueDataProvider;
        private readonly IEnumerable<IUpdateRetryQueueItemHandler> updateItemHandlers;

        public KafkaRetryDurableQueueRepository(
            IKafkaRetryDurableQueueRepositoryProvider retryQueueDataProvider,
            IEnumerable<IUpdateRetryQueueItemHandler> updateItemHandlers,
            IHeadersAdapter headersAdapter,
            KafkaRetryDurablePollingDefinition kafkaRetryDurablePollingDefinition)
        {
            this.retryQueueDataProvider = retryQueueDataProvider;
            this.updateItemHandlers = updateItemHandlers;
            this.headersAdapter = headersAdapter;
            this.kafkaRetryDurablePollingDefinition = kafkaRetryDurablePollingDefinition;
        }

        public async Task<AddIfQueueExistsResult> AddIfQueueExistsAsync(IMessageContext context)
        {
            return await Policy
              .Handle<KafkaRetryException>()
              .WaitAndRetryForeverAsync(retryAttempt => TimeSpan.FromSeconds(retryAttempt > MaxAttempts ? DefaultMaxWaitInSeconds : Math.Pow(2, retryAttempt)))
              .ExecuteAsync(
                async () =>
                {
                    context.Headers.SetString(
                        KafkaRetryDurableConstants.MessageType,
                        $"{context.Message.GetType().FullName}, {context.Message.GetType().Assembly.GetName().Name}"
                    );

                    return await this.AddIfQueueExistsAsync(
                        context,
                        new SaveToQueueInput(
                            new RetryQueueItemMessage(
                                context.Topic,
                                context.PartitionKey,
                                context.Message.SerializeObjectToJson(true),
                                context.Partition.Value,
                                context.Offset.Value,
                                context.Consumer.MessageTimestamp,
                                this.headersAdapter.AdaptToMessageHeaders(context.Headers)
                            ),
                            this.kafkaRetryDurablePollingDefinition.Id,
                            System.Text.UTF8Encoding.UTF8.GetString(context.PartitionKey),
                            RetryQueueStatus.Active,
                            RetryQueueItemStatus.Waiting,
                            SeverityLevel.Unknown, // TODO: which Severity Level should we choose?
                            DateTime.UtcNow,
                            null,
                            DateTime.UtcNow,
                            0,
                            null
                        )
                    ).ConfigureAwait(false);
                }
                ).ConfigureAwait(false);
        }

        public async Task<QueuePendingItemsResult> CheckQueuePendingItemsAsync(QueuePendingItemsInput input)
        {
            if (input.Sort == 0)
            {
                return new QueuePendingItemsResult(QueuePendingItemsResultStatus.NoPendingItems);
            }

            try
            {
                return await this.retryQueueDataProvider.CheckQueuePendingItemsAsync(input).ConfigureAwait(false);
            }
            catch (Exception ex) when (!(ex is KafkaRetryException))
            {
                var kafkaException = this.GetCheckQueueException(
                    $"An error occurred when checking the queue pending items.",
                    input
                );

                //this.policyBuilder.OnDataProviderException(kafkaException);

                throw kafkaException;
            }
        }

        public async Task<IEnumerable<RetryQueue>> GetRetryQueuesAsync(GetQueuesInput getQueuesInput)
        {
            try
            {
                var getQueuesResult = await this.retryQueueDataProvider.GetQueuesAsync(getQueuesInput).ConfigureAwait(false);

                return getQueuesResult?.RetryQueues ?? Enumerable.Empty<RetryQueue>();
            }
            catch (Exception ex)
            {
                var kafkaException = new KafkaRetryException(
                    new RetryError(RetryErrorCode.DataProvider_GetRetryQueues),
                    $"An error ocurred getting the retry queues", ex);

                //this.policyBuilder.OnDataProviderException(kafkaException);

                throw kafkaException;
            }
        }

        public async Task<SaveToQueueResult> SaveToQueueAsync(IMessageContext context, string description)
        {
            return await Policy
                .Handle<KafkaRetryException>(ex => ex.Error.Code != RetryErrorCode.DataProvider_UnrecoverableException)
                .WaitAndRetryForeverAsync(retryAttempt => TimeSpan.FromSeconds(retryAttempt > MaxAttempts ? DefaultMaxWaitInSeconds : Math.Pow(2, retryAttempt)))
                .ExecuteAsync(
                    async () =>
                    {
                        context.Headers.SetString(
                            KafkaRetryDurableConstants.MessageType,
                            $"{context.Message.GetType().FullName}, {context.Message.GetType().Assembly.GetName().Name}" // TODO: reflection is really required here? any other option?
                        );

                        var refDate = DateTime.UtcNow;

                        return await this.SaveToQueueAsync(context,
                           new SaveToQueueInput(
                                new RetryQueueItemMessage(
                                    context.Topic,
                                    context.PartitionKey,
                                    context.Message.SerializeObjectToJson(true),
                                    context.Partition.Value,
                                    context.Offset.Value,
                                    context.Consumer.MessageTimestamp,
                                    this.headersAdapter.AdaptToMessageHeaders(context.Headers)
                                ),
                            this.kafkaRetryDurablePollingDefinition.Id,
                            System.Text.UTF8Encoding.UTF8.GetString(context.PartitionKey), // TODO: this worries me because this convertion can cause data loss.
                            RetryQueueStatus.Active,
                            RetryQueueItemStatus.Waiting,
                            SeverityLevel.Unknown, // TODO: which Severity Level should we choose?
                            refDate,
                            refDate,
                            refDate,
                            0,
                            description
                            )
                       ).ConfigureAwait(false);
                    }
                ).ConfigureAwait(false);
        }

        public async Task UpdateItemAsync(UpdateItemInput input)
        {
            foreach (var handler in this.updateItemHandlers)
            {
                if (handler.CanHandle(input))
                {
                    //this.policyBuilder.OnLog(new Retry.LogMessage(this.policyBuilder.GetSearchGroupKey(), KafkaRetryLogLevel.Info, "RETRY QUEUE STORAGE",
                    // $"An item ({input.ItemId}) will be UPDATED to status {input.Status}"));

                    await Policy
                       .Handle<Exception>()
                       .WaitAndRetryForeverAsync(retryAttempt => TimeSpan.FromSeconds(retryAttempt > MaxAttempts ? DefaultMaxWaitInSeconds : Math.Pow(2, retryAttempt)),
                       (exception, time) =>
                            {
                                //this.policyBuilder.OnLog(new Retry.LogMessage(this.policyBuilder.GetSearchGroupKey(), KafkaRetryLogLevel.Error, "RETRY QUEUE STORAGE",
                                //$"An item ({input.ItemId}) FAILED to update the status to '{input.Status}': {exception?.ToString()}"));

                                var kafkaException = new KafkaRetryException( // TODO: ok, we need to think on how we want to expose this kind of exception in this context to the user.
                                    new RetryError(RetryErrorCode.DataProvider_AddIfQueueExists),
                                    $"An error ocurred while trying to add the item to an existing queue.", exception);

                                //this.policyBuilder.OnDataProviderException(kafkaException);
                            }
                       )
                       .ExecuteAsync(() => handler.UpdateItemAsync(input))
                       .ConfigureAwait(false);

                    return;
                }
            }

            throw new ArgumentException($"None of the handlers is able to update the input {input.GetType().ToString()}");
        }

        private async Task<AddIfQueueExistsResult> AddIfQueueExistsAsync(IMessageContext context, SaveToQueueInput input)
        {
            try
            {
                var checkQueueInput = new CheckQueueInput(input.Message, input.QueueGroupKey);

                var checkQueueResult = await this.retryQueueDataProvider.CheckQueueAsync(checkQueueInput).ConfigureAwait(false);

                if (checkQueueResult.Status == CheckQueueResultStatus.Exists)
                {
                    var saveToQueueResult = await this.retryQueueDataProvider.SaveToQueueAsync(input).ConfigureAwait(false);

                    if (saveToQueueResult.Status == SaveToQueueResultStatus.Added)
                    {
                        //this.policyBuilder.OnMessageAddedToQueue(input.QueueGroupKey, context);

                        return new AddIfQueueExistsResult(AddIfQueueExistsResultStatus.Added);
                    }
                }

                return new AddIfQueueExistsResult(AddIfQueueExistsResultStatus.NoPendingMembers);
            }
            catch (Exception ex)
            {
                var kafkaException = new KafkaRetryException(
                    new RetryError(RetryErrorCode.DataProvider_AddIfQueueExists),
                    $"An error ocurred while trying to add the item to an existing queue.", ex);

                kafkaException.Data.Add(nameof(input.QueueGroupKey), input.QueueGroupKey);

                //this.policyBuilder.OnDataProviderException(kafkaException, context);

                throw kafkaException;
            }
        }

        private KafkaRetryException GetCheckQueueException(string message, QueuePendingItemsInput input)
        {
            var kafkaException = new KafkaRetryException(new RetryError(RetryErrorCode.DataProvider_CheckQueuePendingItems), message);

            kafkaException.Data.Add(nameof(input.QueueId), input.QueueId);
            kafkaException.Data.Add(nameof(input.ItemId), input.ItemId);
            kafkaException.Data.Add(nameof(input.Sort), input.Sort);

            return kafkaException;
        }

        private async Task<SaveToQueueResult> SaveToQueueAsync(IMessageContext context, SaveToQueueInput input)
        {
            try
            {
                var result = await this.retryQueueDataProvider.SaveToQueueAsync(input).ConfigureAwait(false);

                if (result.Status == SaveToQueueResultStatus.Added)
                {
                    //this.policyBuilder.OnMessageAddedToQueue(input.QueueGroupKey, context);
                }
                else if (result.Status == SaveToQueueResultStatus.Created)
                {
                    //this.policyBuilder.OnMessageQueueCreated(input.QueueGroupKey, context);
                }

                return result;
            }
            catch (System.Text.DecoderFallbackException ex)
            {
                var unrecoverableException = new KafkaRetryException(
                    new RetryError(RetryErrorCode.DataProvider_UnrecoverableException),
                    "An unrecoverable error occurred while trying to save the item", ex);

                //this.policyBuilder.OnDataProviderException(unrecoverableException, context);

                throw unrecoverableException;
            }
            catch (Exception ex)
            {
                var retryException = new KafkaRetryException(
                    new RetryError(RetryErrorCode.DataProvider_SaveToQueue),
                    "An error occurred while trying to save the item", ex);

                //this.policyBuilder.OnDataProviderException(retryException, context);

                throw retryException;
            }
        }
    }
}