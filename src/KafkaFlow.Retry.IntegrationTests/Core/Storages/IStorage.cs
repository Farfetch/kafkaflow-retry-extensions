namespace KafkaFlow.Retry.IntegrationTests.Core.Storages
{
    using System.Threading.Tasks;
    using KafkaFlow.Retry.IntegrationTests.Core.Messages;

    internal interface IStorage
    {
        public Task AssertRetryDurableMessageCreationAsync(RetryDurableTestMessage message, int count);

        public Task AssertRetryDurableMessageDoneAsync(RetryDurableTestMessage message);

        public Task AssertRetryDurableMessageRetryingAsync(RetryDurableTestMessage message, int retryCount);

        public void CleanDatabase();
    }
}