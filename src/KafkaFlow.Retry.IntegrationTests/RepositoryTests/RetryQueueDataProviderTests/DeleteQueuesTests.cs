using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using KafkaFlow.Retry.Durable.Repository.Actions.Delete;
using KafkaFlow.Retry.Durable.Repository.Model;
using KafkaFlow.Retry.IntegrationTests.Core.Bootstrappers.Fixtures;
using KafkaFlow.Retry.IntegrationTests.Core.Storages;
using KafkaFlow.Retry.IntegrationTests.Core.Storages.Repositories;

namespace KafkaFlow.Retry.IntegrationTests.RepositoryTests.RetryQueueDataProviderTests;

public class DeleteQueuesTests : RetryQueueDataProviderTestsTemplate
{
    private const RetryQueueStatus QueueStatusToDelete = RetryQueueStatus.Done;
    private const RetryQueueStatus QueueStatusToKeep = RetryQueueStatus.Active;
    private const string SearchGroupKeyToDelete = "SearchGroupKey-RepositoryTests-DeleteQueues";

    public DeleteQueuesTests(BootstrapperRepositoryFixture bootstrapperRepositoryFixture)
        : base(bootstrapperRepositoryFixture)
    {
    }

    [Theory]
    [InlineData(RepositoryType.MongoDb)]
    [InlineData(RepositoryType.SqlServer)]
    [InlineData(RepositoryType.Postgres)]
    public async Task DeleteQueuesAsync_TestingMaxRowsToDelete_DeleteAllEligibleQueuesAfterTwoDeletions(RepositoryType repositoryType)
    {
        // Arrange
        var repository = GetRepository(repositoryType);

        var maxRowsToDelete = 2;
        var maxLastExecutionDateToBeKept = new DateTime(2023, 2, 10, 12, 0, 0);

        var queuesInput = new[] {
            new DeleteQueueTestInput
            {
                EligibleToDelete = true,
                SearchGroupKey = SearchGroupKeyToDelete,
                QueueStatus = QueueStatusToDelete,
                LastExecutionDate = new DateTime(2023, 2, 9, 0, 0, 0)
            },
            new DeleteQueueTestInput
            {
                EligibleToDelete = true,
                SearchGroupKey = SearchGroupKeyToDelete,
                QueueStatus = QueueStatusToDelete,
                LastExecutionDate = new DateTime(2023, 2, 10, 11, 59, 59)
            },
            new DeleteQueueTestInput
            {
                EligibleToDelete = true,
                SearchGroupKey = SearchGroupKeyToDelete,
                QueueStatus = QueueStatusToDelete,
                LastExecutionDate = new DateTime(2021, 2, 10)
            }
        };

        await CreateQueuesAsync(repository, queuesInput);

        var deleteQueuesInput = new DeleteQueuesInput(SearchGroupKeyToDelete, QueueStatusToDelete, maxLastExecutionDateToBeKept, maxRowsToDelete);

        // Act
        var result1 = await repository.RetryQueueDataProvider.DeleteQueuesAsync(deleteQueuesInput);
        var result2 = await repository.RetryQueueDataProvider.DeleteQueuesAsync(deleteQueuesInput);

        // Assert
        result1.Should().NotBeNull();
        result1.TotalQueuesDeleted.Should().Be(2);

        result2.Should().NotBeNull();
        result2.TotalQueuesDeleted.Should().Be(1);
    }

    [Theory]
    [InlineData(RepositoryType.MongoDb)]
    [InlineData(RepositoryType.SqlServer)]
    [InlineData(RepositoryType.Postgres)]
    public async Task DeleteQueuesAsync_WithSeveralScenarios_DeleteAllEligibleQueues(RepositoryType repositoryType)
    {
        // Arrange
        var repository = GetRepository(repositoryType);

        var maxLastExecutionDateToBeKept = new DateTime(2023, 2, 10, 12, 0, 0);

        var queuesInput = new[] {
            new DeleteQueueTestInput
            {
                EligibleToDelete = true,
                SearchGroupKey = SearchGroupKeyToDelete,
                QueueStatus = QueueStatusToDelete,
                LastExecutionDate = new DateTime(2023, 2, 9, 0, 0, 0)
            },
            new DeleteQueueTestInput
            {
                EligibleToDelete = true,
                SearchGroupKey = SearchGroupKeyToDelete,
                QueueStatus = QueueStatusToDelete,
                LastExecutionDate = new DateTime(2023, 2, 10, 11, 59, 59)
            },
            new DeleteQueueTestInput
            {
                EligibleToDelete = false,
                SearchGroupKey = SearchGroupKeyToDelete,
                QueueStatus = QueueStatusToDelete,
                LastExecutionDate = new DateTime(2023, 2, 10, 12, 0, 0)
            },
            new DeleteQueueTestInput
            {
                EligibleToDelete = false,
                SearchGroupKey = SearchGroupKeyToDelete,
                QueueStatus = QueueStatusToKeep,
                LastExecutionDate = new DateTime(2022, 12, 9)
            },
            new DeleteQueueTestInput
            {
                EligibleToDelete = false,
                SearchGroupKey = SearchGroupKeyToDelete,
                QueueStatus = QueueStatusToKeep,
                LastExecutionDate = new DateTime(2023, 2, 13)
            },
            new DeleteQueueTestInput
            {
                EligibleToDelete = false,
                SearchGroupKey = "OtherSearchGroupKey",
                QueueStatus = QueueStatusToDelete,
                LastExecutionDate = new DateTime(2023, 2, 9)
            }
        };

        await CreateQueuesAsync(repository, queuesInput);

        var deleteQueuesInput = new DeleteQueuesInput(SearchGroupKeyToDelete, QueueStatusToDelete, maxLastExecutionDateToBeKept, 100);

        // Act
        var result = await repository.RetryQueueDataProvider.DeleteQueuesAsync(deleteQueuesInput);

        // Assert
        result.Should().NotBeNull();
        result.TotalQueuesDeleted.Should().Be(queuesInput.Count(i => i.EligibleToDelete));
    }

    private async Task CreateQueuesAsync(IRepository repository, DeleteQueueTestInput[] queuesInput)
    {
        foreach (var queueInput in queuesInput)
        {
            var queue = new RetryQueueBuilder()
                .WithSearchGroupKey(queueInput.SearchGroupKey)
                .WithStatus(queueInput.QueueStatus)
                .WithLastExecution(queueInput.LastExecutionDate)
                .WithDefaultItem()
                .Build();

            await repository.CreateQueueAsync(queue);
        }
    }

    private class DeleteQueueTestInput
    {
        public bool EligibleToDelete { get; set; }

        public DateTime LastExecutionDate { get; set; }

        public RetryQueueStatus QueueStatus { get; set; }

        public string SearchGroupKey { get; set; }
    }
}