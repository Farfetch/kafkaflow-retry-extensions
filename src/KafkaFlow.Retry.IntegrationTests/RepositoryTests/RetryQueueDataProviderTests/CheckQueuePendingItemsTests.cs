﻿namespace KafkaFlow.Retry.IntegrationTests.RepositoryTests.RetryQueueDataProviderTests
{
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using KafkaFlow.Retry.Durable.Repository.Actions.Read;
    using KafkaFlow.Retry.Durable.Repository.Model;
    using KafkaFlow.Retry.IntegrationTests.Core.Bootstrappers.Fixtures;
    using KafkaFlow.Retry.IntegrationTests.Core.Storages;
    using KafkaFlow.Retry.IntegrationTests.Core.Storages.Repositories;
    using Xunit;

    public class CheckQueuePendingItemsTests : RetryQueueDataProviderTestsTemplate
    {
        public CheckQueuePendingItemsTests(BootstrapperRepositoryFixture bootstrapperRepositoryFixture)
            : base(bootstrapperRepositoryFixture)
        {
        }

        [Theory]
        [InlineData(RepositoryType.SqlServer)]
        [InlineData(RepositoryType.MongoDb)]
        public async Task CheckQueuePendingItemsAsync_QueueWithOneItem_ReturnsNoPendingItems(RepositoryType repositoryType)
        {
            // Arrange
            var repository = this.GetRepository(repositoryType);

            var expectedResultStatus = QueuePendingItemsResultStatus.NoPendingItems;

            var queue = this.GetDefaultQueue();
            var item = queue.Items.Single();

            await repository.CreateQueueAsync(queue);

            var input = new QueuePendingItemsInput(
              queue.Id,
              item.Id,
              item.Sort
              );

            // Act
            var result = await repository.RetryQueueDataProvider.CheckQueuePendingItemsAsync(input);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be(expectedResultStatus);
        }

        [Theory]
        [InlineData(RepositoryType.MongoDb, QueuePendingItemsResultStatus.NoPendingItems, RetryQueueItemStatus.Done)]
        [InlineData(RepositoryType.SqlServer, QueuePendingItemsResultStatus.NoPendingItems, RetryQueueItemStatus.Done)]
        [InlineData(RepositoryType.MongoDb, QueuePendingItemsResultStatus.NoPendingItems, RetryQueueItemStatus.Cancelled)]
        [InlineData(RepositoryType.SqlServer, QueuePendingItemsResultStatus.NoPendingItems, RetryQueueItemStatus.Cancelled)]
        [InlineData(RepositoryType.MongoDb, QueuePendingItemsResultStatus.HasPendingItems, RetryQueueItemStatus.InRetry)]
        [InlineData(RepositoryType.SqlServer, QueuePendingItemsResultStatus.HasPendingItems, RetryQueueItemStatus.InRetry)]
        [InlineData(RepositoryType.MongoDb, QueuePendingItemsResultStatus.HasPendingItems, RetryQueueItemStatus.Waiting)]
        [InlineData(RepositoryType.SqlServer, QueuePendingItemsResultStatus.HasPendingItems, RetryQueueItemStatus.Waiting)]
        public async Task CheckQueuePendingItemsAsync_QueueWithTwoItems_ReturnsExpectedPendingItemsStatus(
            RepositoryType repositoryType,
            QueuePendingItemsResultStatus expectedResultStatus,
            RetryQueueItemStatus firstItemStatus)
        {
            // Arrange
            var repository = this.GetRepository(repositoryType);

            var queue = new RetryQueueBuilder()
                .CreateItem()
                    .WithStatus(firstItemStatus)
                    .AddItem()
                .CreateItem()
                    .WithWaitingStatus()
                    .AddItem()
                .Build();

            await repository.CreateQueueAsync(queue);

            var item = this.GetQueueLastItem(queue);

            var input = new QueuePendingItemsInput(
                queue.Id,
                item.Id,
                item.Sort
                );

            // Act
            var result = await repository.RetryQueueDataProvider.CheckQueuePendingItemsAsync(input);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be(expectedResultStatus);
        }
    }
}