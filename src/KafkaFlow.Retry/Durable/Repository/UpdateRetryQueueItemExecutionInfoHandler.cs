namespace KafkaFlow.Retry.Durable.Repository
{
    using System;
    using System.Threading.Tasks;
    using Dawn;
    using KafkaFlow.Retry.Durable;
    using KafkaFlow.Retry.Durable.Repository.Actions.Update;

    internal class UpdateRetryQueueItemExecutionInfoHandler : IUpdateRetryQueueItemHandler
    {
        //private readonly RetryPolicyBuilder<TKey, TResult> policyBuilder;
        private readonly IKafkaRetryDurableQueueRepositoryProvider retryQueueDataProvider;

        public UpdateRetryQueueItemExecutionInfoHandler(
            IKafkaRetryDurableQueueRepositoryProvider retryQueueDataProvider)
        {
            this.retryQueueDataProvider = retryQueueDataProvider;
        }

        public bool CanHandle(UpdateItemInput input) => input is UpdateItemExecutionInfoInput;

        public async Task UpdateItemAsync(UpdateItemInput input)
        {
            Guard.Argument(input, nameof(input)).Compatible<UpdateItemExecutionInfoInput>(i => $"The input have to be a {nameof(UpdateItemExecutionInfoInput)}.");

            var updateItemExecutionInfoInput = input as UpdateItemExecutionInfoInput;

            try
            {
                var result = await this.retryQueueDataProvider.UpdateItemExecutionInfoAsync(updateItemExecutionInfoInput).ConfigureAwait(false);

                if (result.Status != UpdateItemResultStatus.Updated)
                {
                    var kafkaException = new KafkaRetryException(
                        new RetryError(RetryErrorCode.DataProvider_UpdateItem),
                        $"{result.Status} while updating the item execution info."
                    );

                    kafkaException.Data.Add(nameof(updateItemExecutionInfoInput.QueueId), updateItemExecutionInfoInput.QueueId);
                    kafkaException.Data.Add(nameof(updateItemExecutionInfoInput.ItemId), updateItemExecutionInfoInput.ItemId);
                    kafkaException.Data.Add(nameof(updateItemExecutionInfoInput.Status), updateItemExecutionInfoInput.Status);

                    //this.policyBuilder.OnDataProviderException(kafkaException);

                    throw kafkaException;
                }
            }
            catch (Exception ex) when (!(ex is KafkaRetryException))
            {
                var kafkaException = new KafkaRetryException(
                  new RetryError(RetryErrorCode.DataProvider_UpdateItem),
                  $"An error ocurred while trying to update the item execution info.", ex
                );

                kafkaException.Data.Add(nameof(updateItemExecutionInfoInput.QueueId), updateItemExecutionInfoInput.QueueId);
                kafkaException.Data.Add(nameof(updateItemExecutionInfoInput.ItemId), updateItemExecutionInfoInput.ItemId);
                kafkaException.Data.Add(nameof(updateItemExecutionInfoInput.Status), updateItemExecutionInfoInput.Status);

                //this.policyBuilder.OnDataProviderException(kafkaException);

                throw kafkaException;
            }
        }
    }
}