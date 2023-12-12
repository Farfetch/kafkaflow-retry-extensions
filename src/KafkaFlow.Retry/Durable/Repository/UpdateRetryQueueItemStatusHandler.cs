using System;
using System.Threading.Tasks;
using Dawn;
using KafkaFlow.Retry.Durable;
using KafkaFlow.Retry.Durable.Repository.Actions.Update;

namespace KafkaFlow.Retry.Durable.Repository;

internal class UpdateRetryQueueItemStatusHandler : IUpdateRetryQueueItemHandler
{
    private readonly IRetryDurableQueueRepositoryProvider retryDurableQueueRepositoryProvider;

    public UpdateRetryQueueItemStatusHandler(IRetryDurableQueueRepositoryProvider retryDurableQueueRepositoryProvider)
    {
            Guard.Argument(retryDurableQueueRepositoryProvider).NotNull();

            this.retryDurableQueueRepositoryProvider = retryDurableQueueRepositoryProvider;
        }

    public bool CanHandle(UpdateItemInput input) => input is UpdateItemStatusInput;

    public async Task UpdateItemAsync(UpdateItemInput input)
    {
            Guard.Argument(input, nameof(input)).Compatible<UpdateItemStatusInput>(i => $"The input have to be a {nameof(UpdateItemStatusInput)}.");

            var updateItemStatusInput = input as UpdateItemStatusInput;

            try
            {
                await retryDurableQueueRepositoryProvider.UpdateItemStatusAsync(updateItemStatusInput).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var kafkaException = new RetryDurableException(
                  new RetryError(RetryErrorCode.DataProvider_UpdateItem),
                  $"An error ocurred while updating the retry queue item status.", ex);

                throw kafkaException;
            }
        }
}