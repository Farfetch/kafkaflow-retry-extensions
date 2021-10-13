namespace KafkaFlow.Retry.IntegrationTests.Core.Storages
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using KafkaFlow.Retry.IntegrationTests.Core.Messages;
    using KafkaFlow.Retry.IntegrationTests.Core.Storages.Models;
    using KafkaFlow.Retry.IntegrationTests.Core.Storages.Repositories;
    using MongoDB.Driver;
    using Xunit;

    public class RetryDurableLatestConsumptionPhysicalStorageAssert : IPhysicalStorageAssert
    {
        private readonly IRepositoryProvider repositoryProvider;

        public RetryDurableLatestConsumptionPhysicalStorageAssert(IRepositoryProvider repositoryProvider)
        {
            this.repositoryProvider = repositoryProvider;
        }

        public async Task AssertRetryDurableMessageCreationAsync(Type repositoryType, RetryDurableTestMessage message, int count)
        {
            var retryQueue = await this
                .repositoryProvider
                .GetRepositoryOfType(repositoryType)
                .GetRetryQueueAsync(message)
                .ConfigureAwait(false);

            Assert.True(retryQueue.Id != Guid.Empty, "Retry Durable Creation Get Retry Queue cannot be asserted.");

            var retryQueueItems = await this
                .repositoryProvider
                .GetRepositoryOfType(repositoryType)
                .GetRetryQueueItemsAsync(retryQueue.Id, rqi =>
                {
                    return rqi.Count() != count;
                })
                .ConfigureAwait(false);

            Assert.True(retryQueueItems != null, "Retry Durable Creation Get Retry Queue Item Message cannot be asserted.");

            Assert.Equal(0, retryQueueItems.Sum(i => i.AttemptsCount));
            Assert.Equal(retryQueueItems.Count() - 1, retryQueueItems.Max(i => i.Sort));
            Assert.True(Enum.Equals(retryQueue.Status, RetryQueueStatusTestModel.Active));
            Assert.All(retryQueueItems, i => Enum.Equals(i.Status, RetryQueueItemStatusTestModel.Waiting));
        }

        public async Task AssertRetryDurableMessageDoneAsync(Type repositoryType, RetryDurableTestMessage message)
        {
            var retryQueue = await this
                .repositoryProvider
                .GetRepositoryOfType(repositoryType)
                .GetRetryQueueAsync(message)
                .ConfigureAwait(false);

            Assert.True(retryQueue.Id != Guid.Empty, "Retry Durable Done Get Retry Queue cannot be asserted.");

            var retryQueueItems = await this
                .repositoryProvider
                .GetRepositoryOfType(repositoryType)
                .GetRetryQueueItemsAsync(
                retryQueue.Id,
                rqi =>
                {
                    return rqi.OrderBy(x => x.Sort).Last().Status != RetryQueueItemStatusTestModel.Done;
                }).ConfigureAwait(false);

            Assert.True(retryQueueItems != null, "Retry Durable Done Get Retry Queue Item Message cannot be asserted.");

            Assert.True(Enum.Equals(retryQueue.Status, RetryQueueStatusTestModel.Done));
        }

        public async Task AssertRetryDurableMessageRetryingAsync(Type repositoryType, RetryDurableTestMessage message, int retryCount)
        {
            var retryQueue = await this
                .repositoryProvider
                .GetRepositoryOfType(repositoryType)
                .GetRetryQueueAsync(message).ConfigureAwait(false);

            Assert.True(retryQueue.Id != Guid.Empty, "Retry Durable Retrying Get Retry Queue cannot be asserted.");

            var retryQueueItems = await this
                .repositoryProvider
                .GetRepositoryOfType(repositoryType)
                .GetRetryQueueItemsAsync(
                retryQueue.Id,
                rqi =>
                {
                    return rqi.OrderBy(x => x.Sort).Last().AttemptsCount != retryCount;
                }).ConfigureAwait(false);

            Assert.True(retryQueueItems != null, "Retry Durable Retrying Get Retry Queue Item Message cannot be asserted.");

            Assert.True(Enum.Equals(retryQueue.Status, RetryQueueStatusTestModel.Active));
            Assert.Equal(retryQueueItems.Count() - 1, retryQueueItems.Max(i => i.Sort));
            Assert.Equal(RetryQueueItemStatusTestModel.Waiting, retryQueueItems.OrderBy(x => x.Sort).Last().Status);
            Assert.All(retryQueueItems.OrderByDescending(x => x.Sort).Skip(1), i => Enum.Equals(i.Status, RetryQueueItemStatusTestModel.Cancelled));
        }
    }
}