namespace KafkaFlow.Retry.Durable.Repository
{
    using System;
    using System.Threading.Tasks;
    using Dawn;
    using KafkaFlow.Retry.Durable;
    using KafkaFlow.Retry.Durable.Repository.Actions.Update;

    internal class UpdateRetryQueueItemStatusHandler : IUpdateRetryQueueItemHandler
    {
        // private readonly RetryPolicyBuilder<TKey, TResult> policyBuilder;
        private readonly IRetryQueueDataProvider retryQueueDataProvider;

        public UpdateRetryQueueItemStatusHandler(IRetryQueueDataProvider retryQueueDataProvider)
        {
            this.retryQueueDataProvider = retryQueueDataProvider;
            //this.policyBuilder = policyBuilder;
        }

        public bool CanHandle(UpdateItemInput input) => input is UpdateItemStatusInput;

        public async Task UpdateItemAsync(UpdateItemInput input)
        {
            Guard.Argument(input, nameof(input)).Compatible<UpdateItemStatusInput>(i => $"The input have to be a {nameof(UpdateItemStatusInput)}.");

            var updateItemStatusInput = input as UpdateItemStatusInput;

            try
            {
                await this.retryQueueDataProvider.UpdateItemStatusAsync(updateItemStatusInput).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var kafkaException = new KafkaRetryException(
                  new RetryError(RetryErrorCode.DataProvider_UpdateItem),
                  $"An error ocurred while updating the retry queue item status.", ex);

                //this.policyBuilder.OnDataProviderException(kafkaException);

                throw kafkaException;
            }
        }
    }
}