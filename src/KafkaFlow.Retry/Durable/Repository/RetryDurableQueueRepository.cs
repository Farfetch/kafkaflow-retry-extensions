namespace KafkaFlow.Retry.Durable.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Dawn;
    using KafkaFlow.Retry.Durable;
    using KafkaFlow.Retry.Durable.Common;
    using KafkaFlow.Retry.Durable.Definitions;
    using KafkaFlow.Retry.Durable.Encoders;
    using KafkaFlow.Retry.Durable.Repository.Actions.Create;
    using KafkaFlow.Retry.Durable.Repository.Actions.Read;
    using KafkaFlow.Retry.Durable.Repository.Actions.Update;
    using KafkaFlow.Retry.Durable.Repository.Adapters;
    using KafkaFlow.Retry.Durable.Repository.Model;
    using Polly;

    internal class RetryDurableQueueRepository : IRetryDurableQueueRepository
    {
        private const int DefaultMaxWaitInSeconds = 60;
        private const int MaxAttempts = 6;
        private readonly IMessageAdapter messageAdapter;
        private readonly IMessageHeadersAdapter messageHeadersAdapter;
        private readonly RetryDurablePollingDefinition retryDurablePollingDefinition;
        private readonly IRetryDurableQueueRepositoryProvider retryDurableRepositoryProvider;
        private readonly IEnumerable<IUpdateRetryQueueItemHandler> updateItemHandlers;
        private readonly IUtf8Encoder utf8Encoder;

        public RetryDurableQueueRepository(
            IRetryDurableQueueRepositoryProvider retryDurableRepositoryProvider,
            IEnumerable<IUpdateRetryQueueItemHandler> updateItemHandlers,
            IMessageHeadersAdapter messageHeadersAdapter,
            IMessageAdapter messageAdapter,
            IUtf8Encoder utf8Encoder,
            RetryDurablePollingDefinition retryDurablePollingDefinition)
        {
            Guard.Argument(retryDurableRepositoryProvider).NotNull("Retry durable requires a repository to be defined");
            Guard.Argument(updateItemHandlers).NotNull("At least an update item handler should be defined");
            Guard.Argument(updateItemHandlers.Count()).NotNegative(value => "At least an update item handler should be defined");
            Guard.Argument(messageHeadersAdapter).NotNull();
            Guard.Argument(messageAdapter).NotNull();
            Guard.Argument(utf8Encoder).NotNull();
            Guard.Argument(retryDurablePollingDefinition).NotNull();

            this.retryDurableRepositoryProvider = retryDurableRepositoryProvider;
            this.updateItemHandlers = updateItemHandlers;
            this.messageHeadersAdapter = messageHeadersAdapter;
            this.messageAdapter = messageAdapter;
            this.utf8Encoder = utf8Encoder;
            this.retryDurablePollingDefinition = retryDurablePollingDefinition;
        }

        public async Task<AddIfQueueExistsResult> AddIfQueueExistsAsync(IMessageContext context)
        {
            return await Policy
              .Handle<RetryDurableException>()
              .WaitAndRetryForeverAsync(retryAttempt => TimeSpan.FromSeconds(retryAttempt > MaxAttempts ? DefaultMaxWaitInSeconds : Math.Pow(2, retryAttempt)))
              .ExecuteAsync(
                async () =>
                {
                    // TODO: reflection is really required here? any other option? There are a
                    // Header MessageType which already has de type. If KafkaFLow always create it
                    // we should use it. KafkaFLow uses this approach que get the type.
                    context.Headers.SetString(
                        RetryDurableConstants.MessageType,
                        $"{context.Message.GetType().FullName}, {context.Message.GetType().Assembly.GetName().Name}"
                    );

                    return await this.AddIfQueueExistsAsync(
                        context,
                        new SaveToQueueInput(
                            new RetryQueueItemMessage(
                                context.Topic,
                                context.PartitionKey,
                                this.messageAdapter.AdaptFromKafkaFlowMessage(context.Message),
                                context.Partition.Value,
                                context.Offset.Value,
                                context.Consumer.MessageTimestamp,
                                this.messageHeadersAdapter.AdaptFromKafkaFlowMessageHeaders(context.Headers)
                            ),
                            this.retryDurablePollingDefinition.Id,
                            this.utf8Encoder.Decode(context.PartitionKey), // TODO: this worries me because this convertion can cause data loss.
                            RetryQueueStatus.Active,
                            RetryQueueItemStatus.Waiting,
                            SeverityLevel.Unknown,
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

        public async Task<QueueNewestItemsResult> CheckQueueNewestItemsAsync(QueueNewestItemsInput queueNewestItemsInput)
        {
            try
            {
                return await this.retryDurableRepositoryProvider.CheckQueueNewestItemsAsync(queueNewestItemsInput).ConfigureAwait(false);
            }
            catch (Exception ex) when (!(ex is RetryDurableException))
            {
                var kafkaException = this.GetCheckQueueException(
                    $"An error occurred when checking the queue pending items.",
                    queueNewestItemsInput
                );

                //this.policyBuilder.OnDataProviderException(kafkaException);

                throw kafkaException;
            }
        }

        public async Task<QueuePendingItemsResult> CheckQueuePendingItemsAsync(QueuePendingItemsInput queuePendingItemsInput)
        {
            if (queuePendingItemsInput.Sort == 0)
            {
                return new QueuePendingItemsResult(QueuePendingItemsResultStatus.NoPendingItems);
            }

            try
            {
                return await this.retryDurableRepositoryProvider.CheckQueuePendingItemsAsync(queuePendingItemsInput).ConfigureAwait(false);
            }
            catch (Exception ex) when (!(ex is RetryDurableException))
            {
                var kafkaException = this.GetCheckQueueException(
                    $"An error occurred when checking the queue pending items.",
                    queuePendingItemsInput
                );

                //this.policyBuilder.OnDataProviderException(kafkaException);

                throw kafkaException;
            }
        }

        public async Task<IEnumerable<RetryQueue>> GetRetryQueuesAsync(GetQueuesInput getQueuesInput)
        {
            try
            {
                var getQueuesResult = await this.retryDurableRepositoryProvider.GetQueuesAsync(getQueuesInput).ConfigureAwait(false);

                return getQueuesResult?.RetryQueues ?? Enumerable.Empty<RetryQueue>();
            }
            catch (Exception ex)
            {
                var kafkaException = new RetryDurableException(
                    new RetryError(RetryErrorCode.DataProvider_GetRetryQueues),
                    $"An error ocurred getting the retry queues", ex);

                //this.policyBuilder.OnDataProviderException(kafkaException);

                throw kafkaException;
            }
        }

        public async Task<SaveToQueueResult> SaveToQueueAsync(IMessageContext context, string description)
        {
            return await Policy
                .Handle<RetryDurableException>(ex => ex.Error.Code != RetryErrorCode.DataProvider_UnrecoverableException)
                .WaitAndRetryForeverAsync(retryAttempt => TimeSpan.FromSeconds(retryAttempt > MaxAttempts ? DefaultMaxWaitInSeconds : Math.Pow(2, retryAttempt)))
                .ExecuteAsync(
                    async () =>
                    {
                        // TODO: reflection is really required here? any other option? There are a
                        // Header MessageType which already has de type. If KafkaFLow always create
                        // it we should use it. KafkaFLow uses this approach que get the type.
                        context.Headers.SetString(
                            RetryDurableConstants.MessageType,
                            $"{context.Message.GetType().FullName}, {context.Message.GetType().Assembly.GetName().Name}"
                        );

                        var refDate = DateTime.UtcNow;

                        return await this.SaveToQueueAsync(context,
                           new SaveToQueueInput(
                                new RetryQueueItemMessage(
                                    context.Topic,
                                    context.PartitionKey,
                                    this.messageAdapter.AdaptFromKafkaFlowMessage(context.Message),
                                    context.Partition.Value,
                                    context.Offset.Value,
                                    context.Consumer.MessageTimestamp,
                                    this.messageHeadersAdapter.AdaptFromKafkaFlowMessageHeaders(context.Headers)
                                ),
                            this.retryDurablePollingDefinition.Id,
                            this.utf8Encoder.Decode(context.PartitionKey), // TODO: this worries me because this convertion can cause data loss.
                            RetryQueueStatus.Active,
                            RetryQueueItemStatus.Waiting,
                            SeverityLevel.Unknown,
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

        public async Task UpdateItemAsync(UpdateItemInput updateItemInput)
        {
            foreach (var handler in this.updateItemHandlers)
            {
                if (handler.CanHandle(updateItemInput))
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

                                var kafkaException = new RetryDurableException( // TODO: ok, we need to think on how we want to expose this kind of exception in this context to the user.
                                    new RetryError(RetryErrorCode.DataProvider_AddIfQueueExists),
                                    $"An error ocurred while trying to add the item to an existing queue.", exception);

                                //this.policyBuilder.OnDataProviderException(kafkaException);
                            }
                       )
                       .ExecuteAsync(() => handler.UpdateItemAsync(updateItemInput))
                       .ConfigureAwait(false);

                    return;
                }
            }

            throw new ArgumentException($"None of the handlers is able to update the input {updateItemInput.GetType().ToString()}");
        }

        private async Task<AddIfQueueExistsResult> AddIfQueueExistsAsync(IMessageContext context, SaveToQueueInput saveToQueueInput)
        {
            try
            {
                var checkQueueInput = new CheckQueueInput(saveToQueueInput.Message, saveToQueueInput.QueueGroupKey);

                var checkQueueResult = await this.retryDurableRepositoryProvider.CheckQueueAsync(checkQueueInput).ConfigureAwait(false);

                if (checkQueueResult.Status == CheckQueueResultStatus.Exists)
                {
                    var saveToQueueResult = await this.retryDurableRepositoryProvider.SaveToQueueAsync(saveToQueueInput).ConfigureAwait(false);

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
                var kafkaException = new RetryDurableException(
                    new RetryError(RetryErrorCode.DataProvider_AddIfQueueExists),
                    $"An error ocurred while trying to add the item to an existing queue.", ex);

                kafkaException.Data.Add(nameof(saveToQueueInput.QueueGroupKey), saveToQueueInput.QueueGroupKey);

                //this.policyBuilder.OnDataProviderException(kafkaException, context);

                throw kafkaException;
            }
        }

        private RetryDurableException GetCheckQueueException(string message, QueuePendingItemsInput input)
        {
            var kafkaException = new RetryDurableException(new RetryError(RetryErrorCode.DataProvider_CheckQueuePendingItems), message);

            kafkaException.Data.Add(nameof(input.QueueId), input.QueueId);
            kafkaException.Data.Add(nameof(input.ItemId), input.ItemId);
            kafkaException.Data.Add(nameof(input.Sort), input.Sort);

            return kafkaException;
        }

        private RetryDurableException GetCheckQueueException(string message, QueueNewestItemsInput input)
        {
            var kafkaException = new RetryDurableException(new RetryError(RetryErrorCode.DataProvider_CheckQueuePendingItems), message);

            kafkaException.Data.Add(nameof(input.QueueId), input.QueueId);
            kafkaException.Data.Add(nameof(input.ItemId), input.ItemId);
            kafkaException.Data.Add(nameof(input.Sort), input.Sort);

            return kafkaException;
        }

        private async Task<SaveToQueueResult> SaveToQueueAsync(IMessageContext context, SaveToQueueInput input)
        {
            try
            {
                var result = await this.retryDurableRepositoryProvider.SaveToQueueAsync(input).ConfigureAwait(false);

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
                var unrecoverableException = new RetryDurableException(
                    new RetryError(RetryErrorCode.DataProvider_UnrecoverableException),
                    "An unrecoverable error occurred while trying to save the item", ex);

                //this.policyBuilder.OnDataProviderException(unrecoverableException, context);

                throw unrecoverableException;
            }
            catch (Exception ex)
            {
                var retryException = new RetryDurableException(
                    new RetryError(RetryErrorCode.DataProvider_SaveToQueue),
                    "An error occurred while trying to save the item", ex);

                //this.policyBuilder.OnDataProviderException(retryException, context);

                throw retryException;
            }
        }
    }
}