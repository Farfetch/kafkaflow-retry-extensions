using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using KafkaFlow.Retry.Durable.Repository.Actions.Update;
using KafkaFlow.Retry.Durable.Repository.Model;
using KafkaFlow.Retry.IntegrationTests.Core.Bootstrappers.Fixtures;
using KafkaFlow.Retry.IntegrationTests.Core.Storages;
using KafkaFlow.Retry.IntegrationTests.Core.Storages.Repositories;

namespace KafkaFlow.Retry.IntegrationTests.RepositoryTests.RetryQueueDataProviderTests;

public class UpdateItemsTests : RetryQueueDataProviderTestsTemplate
{
    public UpdateItemsTests(BootstrapperRepositoryFixture bootstrapperRepositoryFixture)
        : base(bootstrapperRepositoryFixture)
    {
    }

    [Theory]
    [InlineData(RepositoryType.MongoDb)]
    [InlineData(RepositoryType.SqlServer)]
    [InlineData(RepositoryType.Postgres)]
    public async Task UpdateItemsTestsAsync_ExistingItemsInWaitingState_ReturnsItemIsNotTheFirstWaitingInQueue(RepositoryType repositoryType)
    {
        // Arrange
        var repository = GetRepository(repositoryType);

        var expectedItemStatus = RetryQueueItemStatus.Waiting;

        var queue = new RetryQueueBuilder()
            .CreateItem().WithWaitingStatus().AddItem()
            .CreateItem().WithWaitingStatus().AddItem()
            .Build();

        await repository.CreateQueueAsync(queue);
        var lastItem = GetQueueLastItem(queue);

        var inputUpdate = new UpdateItemsInput(new[] { lastItem.Id }, RetryQueueItemStatus.Cancelled);

        // Act
        var result = await repository.RetryQueueDataProvider.UpdateItemsAsync(inputUpdate);

        // Assert
        result.Results.First().Status.Should().Be(UpdateItemResultStatus.ItemIsNotTheFirstWaitingInQueue);

        var actualQueue = await repository.GetAllRetryQueueDataAsync(queue.QueueGroupKey);
        actualQueue.Should().NotBeNull();
        actualQueue.Items.Should().HaveCount(2);
        actualQueue.Items.ElementAt(0).Status.Should().Be(expectedItemStatus);
        actualQueue.Items.ElementAt(1).Status.Should().Be(expectedItemStatus);
    }

    [Theory]
    [InlineData(RepositoryType.MongoDb)]
    [InlineData(RepositoryType.SqlServer)]
    [InlineData(RepositoryType.Postgres)]
    public async Task UpdateItemsTestsAsync_ExistingItemsInWaitingState_ReturnsUpdatedStatus(RepositoryType repositoryType)
    {
        // Arrange
        var repository = GetRepository(repositoryType);
        var expectedItemStatus = RetryQueueItemStatus.Cancelled;
        var expectedSecondItemStatus = RetryQueueItemStatus.Waiting;

        var queue = new RetryQueueBuilder()
            .CreateItem().WithWaitingStatus().AddItem()
            .CreateItem().WithWaitingStatus().AddItem()
            .Build();

        await repository.CreateQueueAsync(queue);
        var firstItem = GetQueueFirstItem(queue);

        var inputUpdate = new UpdateItemsInput(new[] { firstItem.Id }, RetryQueueItemStatus.Cancelled);

        // Act
        var result = await repository.RetryQueueDataProvider.UpdateItemsAsync(inputUpdate);

        // Assert
        result.Results.First().Status.Should().Be(UpdateItemResultStatus.Updated);

        var actualQueue = await repository.GetAllRetryQueueDataAsync(queue.QueueGroupKey);
        actualQueue.Should().NotBeNull();
        actualQueue.Items.Should().HaveCount(2);
        actualQueue.Items.First().Status.Should().Be(expectedItemStatus);
        actualQueue.Items.Last().Status.Should().Be(expectedSecondItemStatus);
    }

    [Theory]
    [InlineData(RepositoryType.MongoDb)]
    [InlineData(RepositoryType.SqlServer)]
    [InlineData(RepositoryType.Postgres)]
    public async Task UpdateItemsTestsAsync_ExistingItemWithNotInWaitingState_ReturnsItemIsNotInWaitingState(RepositoryType repositoryType)
    {
        // Arrange
        var repository = GetRepository(repositoryType);
        var expectedItemStatus = RetryQueueItemStatus.InRetry;

        var queue = new RetryQueueBuilder()
            .CreateItem().WithInRetryStatus().AddItem()
            .Build();

        await repository.CreateQueueAsync(queue);
        var item = queue.Items.Single();

        var inputUpdate = new UpdateItemsInput(new[] { item.Id }, RetryQueueItemStatus.Cancelled);

        // Act
        var result = await repository.RetryQueueDataProvider.UpdateItemsAsync(inputUpdate);

        // Assert
        result.Results.First().Status.Should().Be(UpdateItemResultStatus.ItemIsNotInWaitingState);

        var actualQueue = await repository.GetAllRetryQueueDataAsync(queue.QueueGroupKey);
        actualQueue.Should().NotBeNull();
        actualQueue.Items.Should().HaveCount(1);
        actualQueue.Items.Single().Status.Should().Be(expectedItemStatus);
    }

    [Theory]
    [InlineData(RepositoryType.MongoDb)]
    [InlineData(RepositoryType.SqlServer)]
    [InlineData(RepositoryType.Postgres)]
    public async Task UpdateItemsTestsAsync_ExistingItemWithStatusNotCancelled_ReturnsUpdatedStatusNotAllowed(RepositoryType repositoryType)
    {
        // Arrange
        var repository = GetRepository(repositoryType);
        var InvalidItemStatus = RetryQueueItemStatus.Done;
        var expectedItemStatus = RetryQueueItemStatus.Waiting;

        var queue = GetDefaultQueue();

        await repository.CreateQueueAsync(queue);

        var item = queue.Items.Single();

        var inputUpdate = new UpdateItemsInput(new[] { item.Id }, InvalidItemStatus);

        // Act
        var result = await repository.RetryQueueDataProvider.UpdateItemsAsync(inputUpdate);

        // Assert
        result.Results.First().Status.Should().Be(UpdateItemResultStatus.UpdateIsNotAllowed);

        var actualQueue = await repository.GetAllRetryQueueDataAsync(queue.QueueGroupKey);
        actualQueue.Should().NotBeNull();
        actualQueue.Items.Should().HaveCount(1);
        actualQueue.Items.Single().Status.Should().Be(expectedItemStatus);
    }

    [Theory]
    [InlineData(RepositoryType.MongoDb)]
    [InlineData(RepositoryType.SqlServer)]
    [InlineData(RepositoryType.Postgres)]
    public async Task UpdateItemsTestsAsync_NonExistingItem_ReturnsItemNotFoundStatus(RepositoryType repositoryType)
    {
        // Arrange
        var repository = GetRepository(repositoryType);

        var inputUpdate = new UpdateItemsInput(new[] { Guid.NewGuid() }, RetryQueueItemStatus.Cancelled);

        // Act
        var result = await repository.RetryQueueDataProvider.UpdateItemsAsync(inputUpdate);

        // Assert
        result.Results.First().Status.Should().Be(UpdateItemResultStatus.ItemNotFound);
    }
}