namespace KafkaFlow.Retry.IntegrationTests.Core.Storages.Assertion
{
    using System.Threading.Tasks;
    using KafkaFlow.Retry.IntegrationTests.Core.Messages;
    using KafkaFlow.Retry.IntegrationTests.Core.Storages.Repositories;

    internal interface IPhysicalStorageAssert
    {
        Task AssertRetryDurableMessageCreationAsync(RepositoryType repositoryType, RetryDurableTestMessage message, int count);

        Task AssertRetryDurableMessageDoneAsync(RepositoryType repositoryType, RetryDurableTestMessage message);

        Task AssertRetryDurableMessageRetryingAsync(RepositoryType repositoryType, RetryDurableTestMessage message, int retryCount);
    }
}