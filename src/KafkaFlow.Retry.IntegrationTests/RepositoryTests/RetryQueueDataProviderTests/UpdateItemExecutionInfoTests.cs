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

public class UpdateItemExecutionInfoTests : RetryQueueDataProviderTestsTemplate
{
    public UpdateItemExecutionInfoTests(BootstrapperRepositoryFixture bootstrapperRepositoryFixture)
        : base(bootstrapperRepositoryFixture)
    {
        }

    [Theory]
    [InlineData(RepositoryType.MongoDb, RetryQueueItemStatus.InRetry)]
    [InlineData(RepositoryType.SqlServer, RetryQueueItemStatus.InRetry)]
    [InlineData(RepositoryType.Postgres, RetryQueueItemStatus.InRetry)]
    [InlineData(RepositoryType.MongoDb, RetryQueueItemStatus.Waiting)]
    [InlineData(RepositoryType.SqlServer, RetryQueueItemStatus.Waiting)]
    [InlineData(RepositoryType.Postgres, RetryQueueItemStatus.Waiting)]
    public async Task UpdateItemExecutionInfoAsync_UpdateStatus_ReturnsUpdatedStatus(RepositoryType repositoryType, RetryQueueItemStatus expectedItemStatus)
    {
            // Arrange
            var repository = GetRepository(repositoryType);

            var expectedAttemptsCount = 2;
            var expectedLastExecution = new DateTime(2022, 3, 31).ToUniversalTime();
            var expectedDescription = "ExpectedDescription";

            var queue = new RetryQueueBuilder()
                .CreateItem()
                    .WithInRetryStatus()
                    .AddItem()
                .Build();

            var item = queue.Items.Single();

            await repository.CreateQueueAsync(queue);

            var input = new UpdateItemExecutionInfoInput(queue.Id, item.Id, expectedItemStatus, expectedAttemptsCount, expectedLastExecution, expectedDescription);

            // Act
            var result = await repository.RetryQueueDataProvider.UpdateItemExecutionInfoAsync(input);

            // Assert
            result.Status.Should().Be(UpdateItemResultStatus.Updated);

            var actualQueue = await repository.GetAllRetryQueueDataAsync(queue.QueueGroupKey);
            actualQueue.Should().NotBeNull();
            actualQueue.Status.Should().Be(RetryQueueStatus.Active);
            actualQueue.LastExecution.Date.Should().Be(expectedLastExecution.Date);
            actualQueue.Items.Should().HaveCount(1);

            var actualItem = actualQueue.Items.Single(i => i.Id == item.Id);
            actualItem.Status.Should().Be(expectedItemStatus);
            actualItem.LastExecution.Value.Date.Should().Be(expectedLastExecution.Date);
            actualItem.Sort.Should().Be(item.Sort);
            actualItem.AttemptsCount.Should().Be(expectedAttemptsCount);
            actualItem.Description.Should().Be(expectedDescription);
        }

    [Theory]
    [InlineData(RepositoryType.MongoDb)]
    [InlineData(RepositoryType.SqlServer)]
    [InlineData(RepositoryType.Postgres)]
    public async Task UpdateItemExecutionInfoAsync_UpdateToDone_QueueWithoutAllItemsDone_ReturnsUpdatedStatus(RepositoryType repositoryType)
    {
            // Arrange
            var repository = GetRepository(repositoryType);

            var expectedAttemptsCount = 2;
            var expectedLastExecution = new DateTime(2022, 3, 31).ToUniversalTime();
            var expectedItemStatus = RetryQueueItemStatus.Done;
            var expectedDescription = "ExpectedDescription";

            var queue = new RetryQueueBuilder()
               .CreateItem()
                   .WithWaitingStatus()
                   .AddItem()
               .CreateItem()
                   .WithWaitingStatus()
                   .AddItem()
               .Build();

            await repository.CreateQueueAsync(queue);
            var firstItem = GetQueueFirstItem(queue);

            var input = new UpdateItemExecutionInfoInput(queue.Id, firstItem.Id, expectedItemStatus, expectedAttemptsCount, expectedLastExecution, expectedDescription);

            // Act
            var result = await repository.RetryQueueDataProvider.UpdateItemExecutionInfoAsync(input);

            // Assert
            result.Status.Should().Be(UpdateItemResultStatus.Updated);

            var actualQueue = await repository.GetAllRetryQueueDataAsync(queue.QueueGroupKey);
            actualQueue.Should().NotBeNull();
            actualQueue.Status.Should().Be(RetryQueueStatus.Active);
            actualQueue.LastExecution.Date.Should().Be(expectedLastExecution.Date);

            actualQueue.Items.Should().HaveCount(2);
            var actualFirstItem = actualQueue.Items.Single(i => i.Id == firstItem.Id);

            actualFirstItem.Status.Should().Be(expectedItemStatus);
            actualFirstItem.LastExecution.Value.Date.Should().Be(expectedLastExecution.Date);
            actualFirstItem.Sort.Should().Be(firstItem.Sort);
            actualFirstItem.AttemptsCount.Should().Be(expectedAttemptsCount);
            actualFirstItem.Description.Should().Be(expectedDescription);
        }

    [Theory]
    [InlineData(RepositoryType.MongoDb)]
    [InlineData(RepositoryType.SqlServer)]
    [InlineData(RepositoryType.Postgres)]
    public async Task UpdateItemExecutionInfoAsync_UpdateToDone_ReturnsUpdatedStatusQueueWithAllItemsDone(RepositoryType repositoryType)
    {
            // Arrange
            var repository = GetRepository(repositoryType);

            var expectedAttemptsCount = 5;
            var expectedLastExecution = new DateTime(2022, 3, 31).ToUniversalTime();
            var expectedItemStatus = RetryQueueItemStatus.Done;
            var expectedDescription = "ExpectedDescription";

            var queue = GetDefaultQueue();

            await repository.CreateQueueAsync(queue);
            var item = queue.Items.Single();

            var input = new UpdateItemExecutionInfoInput(queue.Id, item.Id, expectedItemStatus, expectedAttemptsCount, expectedLastExecution, expectedDescription);

            // Act
            var result = await repository.RetryQueueDataProvider.UpdateItemExecutionInfoAsync(input);

            // Assert
            result.Status.Should().Be(UpdateItemResultStatus.Updated);

            var actualQueue = await repository.GetAllRetryQueueDataAsync(queue.QueueGroupKey);
            actualQueue.Should().NotBeNull();
            actualQueue.Status.Should().Be(RetryQueueStatus.Done);
            actualQueue.LastExecution.Date.Should().Be(expectedLastExecution.Date);

            actualQueue.Items.Should().HaveCount(1);
            var actualItem = actualQueue.Items.Single();

            actualItem.Status.Should().Be(expectedItemStatus);
            actualItem.LastExecution.Value.Date.Should().Be(expectedLastExecution.Date);
            actualItem.Sort.Should().Be(0);
            actualItem.AttemptsCount.Should().Be(expectedAttemptsCount);
            actualItem.Description.Should().Be(expectedDescription);
        }

    [Theory]
    [InlineData(RepositoryType.MongoDb)]
    [InlineData(RepositoryType.SqlServer)]
    [InlineData(RepositoryType.Postgres)]
    public async Task UpdateItemExecutionInfoAsync_WrongItem_ReturnsItemNotFoundStatus(RepositoryType repositoryType)
    {
            // Arrange
            var repository = GetRepository(repositoryType);

            var notExpectedItemId = Guid.NewGuid();
            var notExpectedAttemptsCount = 5;
            var notExpectedLastExecution = new DateTime(2022, 3, 31).ToUniversalTime();
            var notExpectedItemStatus = RetryQueueItemStatus.Done;
            var expectedDescription = "ExpectedDescription";

            var queue = GetDefaultQueue();

            await repository.CreateQueueAsync(queue);

            var input = new UpdateItemExecutionInfoInput(queue.Id, notExpectedItemId, notExpectedItemStatus, notExpectedAttemptsCount, notExpectedLastExecution, expectedDescription);

            // Act
            var result = await repository.RetryQueueDataProvider.UpdateItemExecutionInfoAsync(input);

            // Assert
            result.Status.Should().Be(UpdateItemResultStatus.ItemNotFound);

            var actualQueue = await repository.GetAllRetryQueueDataAsync(queue.QueueGroupKey);
            actualQueue.Should().NotBeNull();
            actualQueue.Status.Should().Be(queue.Status);
            actualQueue.LastExecution.Should().NotBe(notExpectedLastExecution);

            actualQueue.Items.Should().HaveCount(1);
            var actualItem = actualQueue.Items.Single();

            var itemInfo = queue.Items.Single();

            actualItem.Status.Should().NotBe(notExpectedItemStatus).And.Be(itemInfo.Status);
            actualItem.LastExecution.Should().NotBe(notExpectedLastExecution);
            actualItem.Sort.Should().Be(itemInfo.Sort);
            actualItem.AttemptsCount.Should().NotBe(notExpectedAttemptsCount);
        }

    [Theory]
    [InlineData(RepositoryType.MongoDb, RetryQueueItemStatus.Done)]
    [InlineData(RepositoryType.MongoDb, RetryQueueItemStatus.Waiting)]
    [InlineData(RepositoryType.SqlServer, RetryQueueItemStatus.Done)]
    [InlineData(RepositoryType.SqlServer, RetryQueueItemStatus.Waiting)]
    [InlineData(RepositoryType.Postgres, RetryQueueItemStatus.Done)]
    [InlineData(RepositoryType.Postgres, RetryQueueItemStatus.Waiting)]
    public async Task UpdateItemExecutionInfoAsync_WrongQueue_ReturnsQueueNotFoundStatus(RepositoryType repositoryType, RetryQueueItemStatus notExpectedItemStatus)
    {
            // Arrange
            var repository = GetRepository(repositoryType);

            var wrongQueueId = Guid.NewGuid();
            var notExpectedAttemptsCount = 5;
            var notExpectedLastExecution = new DateTime(2022, 3, 31).ToUniversalTime();
            var expectedDescription = "ExpectedDescription";

            var queue = new RetryQueueBuilder()
                              .CreateItem()
                                .WithInRetryStatus()
                                .AddItem()
                              .Build();

            await repository.CreateQueueAsync(queue);

            var item = queue.Items.Single();

            var input = new UpdateItemExecutionInfoInput(wrongQueueId, item.Id, notExpectedItemStatus, notExpectedAttemptsCount, notExpectedLastExecution, expectedDescription);

            // Act
            var result = await repository.RetryQueueDataProvider.UpdateItemExecutionInfoAsync(input);

            // Assert
            result.Status.Should().Be(UpdateItemResultStatus.QueueNotFound);

            var actualQueue = await repository.GetAllRetryQueueDataAsync(queue.QueueGroupKey);
            actualQueue.Should().NotBeNull();
            actualQueue.Status.Should().Be(queue.Status);
            actualQueue.LastExecution.Should().NotBe(notExpectedLastExecution);

            actualQueue.Items.Should().HaveCount(1);
            var actualItem = actualQueue.Items.Single();

            // TODO: This test will fail as soon as transactions are implemented in mongo (Mongo DB version > 4.x).
            // If that happens just remove the ifs and use the sql lines
            if (repositoryType == RepositoryType.MongoDb)
            {
                actualItem.Status.Should().Be(notExpectedItemStatus).And.Be(notExpectedItemStatus);
                actualItem.AttemptsCount.Should().Be(notExpectedAttemptsCount);
                actualItem.LastExecution.Should().Be(notExpectedLastExecution);
            }

            if (repositoryType is RepositoryType.SqlServer or RepositoryType.Postgres)
            {
                actualItem.Status.Should().NotBe(notExpectedItemStatus).And.Be(item.Status);
                actualItem.AttemptsCount.Should().NotBe(notExpectedAttemptsCount);
                actualItem.LastExecution.Should().NotBe(notExpectedLastExecution);
            }

            actualItem.Sort.Should().Be(item.Sort);
        }
}