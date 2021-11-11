namespace KafkaFlow.Retry.IntegrationTests.Core.Storages.Assertion
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using KafkaFlow.Retry.Durable.Repository.Model;
    using KafkaFlow.Retry.IntegrationTests.Core.Messages;
    using KafkaFlow.Retry.IntegrationTests.Core.Storages.Repositories;
    using MongoDB.Driver;
    using Xunit;

    internal class RetryDurableGuaranteeOrderedConsumptionPhysicalStorageAssert : IPhysicalStorageAssert
    {
        private readonly IRepositoryProvider repositoryProvider;

        public RetryDurableGuaranteeOrderedConsumptionPhysicalStorageAssert(IRepositoryProvider repositoryProvider)
        {
            this.repositoryProvider = repositoryProvider;
        }

        public async Task AssertRetryDurableMessageCreationAsync(RepositoryType repositoryType, RetryDurableTestMessage message, int count)
        {
            var retryQueue = await this
                .repositoryProvider
                .GetRepositoryOfType(repositoryType)
                .GetRetryQueueAsync(message.Key)
                .ConfigureAwait(false);

            Assert.True(retryQueue.Id != Guid.Empty, "Retry Durable Creation Get Retry Queue cannot be asserted.");

            var retryQueueItems = await this
                .repositoryProvider
                .GetRepositoryOfType(repositoryType)
                .GetRetryQueueItemsAsync(retryQueue.Id, rqi => rqi.Count() != count)
                .ConfigureAwait(false);

            Assert.True(retryQueueItems != null, "Retry Durable Creation Get Retry Queue Item Message cannot be asserted.");

            Assert.Equal(0, retryQueueItems.Sum(i => i.AttemptsCount));
            Assert.Equal(retryQueueItems.Count() - 1, retryQueueItems.Max(i => i.Sort));
            Assert.True(Enum.Equals(retryQueue.Status, RetryQueueStatus.Active));
            Assert.All(retryQueueItems, i => Enum.Equals(i.Status, RetryQueueItemStatus.Waiting));
        }

        public async Task AssertRetryDurableMessageDoneAsync(RepositoryType repositoryType, RetryDurableTestMessage message)
        {
            var retryQueue = await this
                .repositoryProvider
                .GetRepositoryOfType(repositoryType)
                .GetRetryQueueAsync(message.Key)
                .ConfigureAwait(false);

            Assert.True(retryQueue.Id != Guid.Empty, "Retry Durable Done Get Retry Queue cannot be asserted.");

            var retryQueueItems = await this
                .repositoryProvider
                .GetRepositoryOfType(repositoryType)
                .GetRetryQueueItemsAsync(
                retryQueue.Id,
                items =>
                {
                    return items.All(item => item.Status != RetryQueueItemStatus.Done);
                }).ConfigureAwait(false);

            Assert.True(retryQueueItems != null, "Retry Durable Done Get Retry Queue Item Message cannot be asserted.");

            Assert.Equal(RetryQueueStatus.Done, retryQueue.Status);
        }

        public async Task AssertRetryDurableMessageRetryingAsync(RepositoryType repositoryType, RetryDurableTestMessage message, int retryCount)
        {
            var retryQueue = await this
                .repositoryProvider
                .GetRepositoryOfType(repositoryType)
                .GetRetryQueueAsync(message.Key).ConfigureAwait(false);

            Assert.True(retryQueue.Id != Guid.Empty, "Retry Durable Retrying Get Retry Queue cannot be asserted.");

            var retryQueueItems = await this
                .repositoryProvider
                .GetRepositoryOfType(repositoryType)
                .GetRetryQueueItemsAsync(
                retryQueue.Id,
                rqi =>
                {
                    return
                    rqi.Single(x => x.Sort == rqi.Min(i => i.Sort)).LastExecution >
                    rqi.Single(x => x.Sort == rqi.Max(i => i.Sort)).LastExecution;
                }).ConfigureAwait(false);

            Assert.True(retryQueueItems != null, "Retry Durable Retrying Get Retry Queue Item Message cannot be asserted.");

            Assert.Equal(retryCount, retryQueueItems.Where(x => x.Sort == 0).Sum(i => i.AttemptsCount));
            Assert.Equal(0, retryQueueItems.Where(x => x.Sort != 0).Sum(i => i.AttemptsCount));
            Assert.Equal(retryQueueItems.Count() - 1, retryQueueItems.Max(i => i.Sort));
            Assert.True(Enum.Equals(retryQueue.Status, RetryQueueStatus.Active));
            Assert.All(retryQueueItems, i => Enum.Equals(i.Status, RetryQueueItemStatus.Waiting));
        }
    }
}