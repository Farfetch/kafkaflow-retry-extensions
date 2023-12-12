using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using KafkaFlow.Retry.Durable.Repository.Actions.Update;
using KafkaFlow.Retry.Durable.Repository.Model;
using KafkaFlow.Retry.IntegrationTests.Core.Bootstrappers.Fixtures;
using KafkaFlow.Retry.IntegrationTests.Core.Storages.Repositories;

namespace KafkaFlow.Retry.IntegrationTests.RepositoryTests.RetryQueueDataProviderTests;

public class UpdateItemStatusTests : RetryQueueDataProviderTestsTemplate
{
    public UpdateItemStatusTests(BootstrapperRepositoryFixture bootstrapperRepositoryFixture)
        : base(bootstrapperRepositoryFixture)
    {
        }

    [Theory]
    [InlineData(RepositoryType.MongoDb)]
    [InlineData(RepositoryType.SqlServer)]
    [InlineData(RepositoryType.Postgres)]
    public async Task UpdateItemStatusAsync_ExistingItem_ReturnsUpdatedStatus(RepositoryType repositoryType)
    {
            // Arrange
            var repository = GetRepository(repositoryType);

            var expectedItemStatus = RetryQueueItemStatus.Done;

            var queue = GetDefaultQueue();

            await repository.CreateQueueAsync(queue);

            var item = queue.Items.Single();
            var itemPreviousStatus = item.Status;

            var inputUpdate = new UpdateItemStatusInput(item.Id, expectedItemStatus);

            // Act
            var result = await repository.RetryQueueDataProvider.UpdateItemStatusAsync(inputUpdate);

            // Assert
            result.Status.Should().Be(UpdateItemResultStatus.Updated);

            var actualQueue = await repository.GetAllRetryQueueDataAsync(queue.QueueGroupKey);
            actualQueue.Should().NotBeNull();
            actualQueue.Items.Should().HaveCount(1);
            actualQueue.Items.Single().Status.Should().Be(expectedItemStatus).And.NotBe(itemPreviousStatus);
        }

    [Theory]
    [InlineData(RepositoryType.MongoDb)]
    [InlineData(RepositoryType.SqlServer)]
    [InlineData(RepositoryType.Postgres)]
    public async Task UpdateItemStatusAsync_NonExistingItem_ReturnsItemNotFoundStatus(RepositoryType repositoryType)
    {
            // Arrange
            var repository = GetRepository(repositoryType);

            var inputUpdate = new UpdateItemStatusInput(Guid.NewGuid(), RetryQueueItemStatus.Done);

            // Act
            var result = await repository.RetryQueueDataProvider.UpdateItemStatusAsync(inputUpdate);

            // Assert
            result.Status.Should().Be(UpdateItemResultStatus.ItemNotFound);
        }
}