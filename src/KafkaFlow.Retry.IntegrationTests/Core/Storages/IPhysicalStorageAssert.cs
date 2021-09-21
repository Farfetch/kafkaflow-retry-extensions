namespace KafkaFlow.Retry.IntegrationTests.Core.Storages
{
    using System;
    using System.Threading.Tasks;
    using KafkaFlow.Retry.IntegrationTests.Core.Messages;

    public interface IPhysicalStorageAssert
    {
        Task AssertRetryDurableMessageCreationAsync(Type repositoryType, RetryDurableTestMessage message, int count);

        Task AssertRetryDurableMessageDoneAsync(Type repositoryType, RetryDurableTestMessage message);

        Task AssertRetryDurableMessageRetryingAsync(Type repositoryType, RetryDurableTestMessage message, int retryCount);
    }
}