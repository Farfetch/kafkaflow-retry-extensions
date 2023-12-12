using System.Threading.Tasks;
using FluentAssertions;
using KafkaFlow.Retry.Durable.Repository.Actions.Read;
using KafkaFlow.Retry.Durable.Repository.Model;
using KafkaFlow.Retry.IntegrationTests.Core.Bootstrappers.Fixtures;
using KafkaFlow.Retry.IntegrationTests.Core.Storages;
using KafkaFlow.Retry.IntegrationTests.Core.Storages.Repositories;

namespace KafkaFlow.Retry.IntegrationTests.RepositoryTests.RetryQueueDataProviderTests;

public class CheckQueueTests : RetryQueueDataProviderTestsTemplate
{
    public CheckQueueTests(BootstrapperRepositoryFixture bootstrapperRepositoryFixture)
        : base(bootstrapperRepositoryFixture)
    {
    }

    [Theory]
    [InlineData(RepositoryType.MongoDb, RetryQueueStatus.Active, CheckQueueResultStatus.Exists)]
    [InlineData(RepositoryType.SqlServer, RetryQueueStatus.Active, CheckQueueResultStatus.Exists)]
    [InlineData(RepositoryType.Postgres, RetryQueueStatus.Active, CheckQueueResultStatus.Exists)]
    [InlineData(RepositoryType.MongoDb, RetryQueueStatus.Done, CheckQueueResultStatus.DoesNotExist)]
    [InlineData(RepositoryType.SqlServer, RetryQueueStatus.Done, CheckQueueResultStatus.DoesNotExist)]
    [InlineData(RepositoryType.Postgres, RetryQueueStatus.Done, CheckQueueResultStatus.DoesNotExist)]
    public async Task CheckQueueAsync_ConsideringQueueStatus_ReturnsExpectedCheckQueueResultStatus(
        RepositoryType repositoryType,
        RetryQueueStatus queueStatus,
        CheckQueueResultStatus expectedCheckQueueResultStatus)
    {
        // Arrange
        var repository = GetRepository(repositoryType);

        var queue = new RetryQueueBuilder()
            .WithStatus(queueStatus)
            .WithDefaultItem()
            .Build();

        var input = new CheckQueueInput(RetryQueueItemBuilder.DefaultItemMessage, queue.QueueGroupKey);

        await repository.CreateQueueAsync(queue);

        // Act
        var result = await repository.RetryQueueDataProvider.CheckQueueAsync(input);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(expectedCheckQueueResultStatus);

        var actualQueue = await repository.GetAllRetryQueueDataAsync(queue.QueueGroupKey);
        actualQueue.Should().NotBeNull();
        actualQueue.Status.Should().Be(queueStatus);
        actualQueue.Items.Should().HaveCount(1);
    }

    [Theory]
    [InlineData(RepositoryType.MongoDb)]
    [InlineData(RepositoryType.SqlServer)]
    [InlineData(RepositoryType.Postgres)]
    public async Task CheckQueueAsync_NonExistingQueue_ReturnsDoesNotExistStatus(RepositoryType repositoryType)
    {
        // Arrange
        var repository = GetRepository(repositoryType);

        var input = new CheckQueueInput(RetryQueueItemBuilder.DefaultItemMessage, "CheckQueue-NonExistingQueue-queueGroupKeyTest");

        // Act
        var result = await repository.RetryQueueDataProvider.CheckQueueAsync(input);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(CheckQueueResultStatus.DoesNotExist);

        var actualQueue = await repository.GetAllRetryQueueDataAsync(input.QueueGroupKey);
        actualQueue.Should().BeNull();
    }
}