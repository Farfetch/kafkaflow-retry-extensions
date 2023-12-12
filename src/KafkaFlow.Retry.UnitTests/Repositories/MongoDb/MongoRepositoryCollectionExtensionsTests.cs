using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using global::KafkaFlow.Retry.Durable.Common;
using global::KafkaFlow.Retry.Durable.Repository.Model;
using global::KafkaFlow.Retry.MongoDb;
using global::KafkaFlow.Retry.MongoDb.Model;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace KafkaFlow.Retry.UnitTests.Repositories.MongoDb;

public class MongoRepositoryCollectionExtensionsTests
{
    private readonly Mock<IMongoCollection<RetryQueueItemDbo>> collection = new Mock<IMongoCollection<RetryQueueItemDbo>>();
    private readonly Mock<IAsyncCursor<RetryQueueItemDbo>> retries = new Mock<IAsyncCursor<RetryQueueItemDbo>>();

    private readonly IEnumerable<RetryQueueItemDbo> retryQueueItemDbos = new List<RetryQueueItemDbo>
    {
        new RetryQueueItemDbo
        {
            Id = Guid.NewGuid(),
            Description = "description",
            CreationDate = DateTime.UtcNow,
            ModifiedStatusDate = DateTime.UtcNow,
            AttemptsCount = 1,
            LastExecution = DateTime.UtcNow,
            Message = new RetryQueueItemMessageDbo(),
            RetryQueueId = Guid.NewGuid(),
            SeverityLevel = SeverityLevel.High,
            Sort = 1,
            Status = RetryQueueItemStatus.Waiting
        }
    };

    public MongoRepositoryCollectionExtensionsTests()
    {
        retries.SetupSequence(d => d.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);

        retries.Setup(d => d.Current).Returns(() => retryQueueItemDbos);

        collection
            .Setup(d => d.FindAsync(
                It.IsAny<FilterDefinition<RetryQueueItemDbo>>(),
                It.IsAny<FindOptions<RetryQueueItemDbo, RetryQueueItemDbo>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(retries.Object);
    }

    [Fact]
    public async Task MongoRepositoryCollectionExtensions_GetAsync_Success()
    {
        // Arrange
        var filter = FilterDefinition<RetryQueueItemDbo>.Empty;

        // Act
        var result = await collection.Object.GetAsync(filter).ConfigureAwait(false);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().BeEquivalentTo(retryQueueItemDbos);
        collection.Verify(d =>
                d.FindAsync(
                    It.IsAny<FilterDefinition<RetryQueueItemDbo>>(),
                    It.IsAny<FindOptions<RetryQueueItemDbo, RetryQueueItemDbo>>(),
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public void MongoRepositoryCollectionExtensions_GetFilters_Success()
    {
        // Act
        var result = collection.Object.GetFilters();

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task MongoRepositoryCollectionExtensions_GetOneAsync_Success()
    {
        // Arrange
        var filter = FilterDefinition<RetryQueueItemDbo>.Empty;

        // Act
        var result = await collection.Object.GetOneAsync(filter).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(retryQueueItemDbos.FirstOrDefault());
        collection.Verify(d =>
                d.FindAsync(
                    It.IsAny<FilterDefinition<RetryQueueItemDbo>>(),
                    It.IsAny<FindOptions<RetryQueueItemDbo, RetryQueueItemDbo>>(),
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public void MongoRepositoryCollectionExtensions_GetSortDefinition_Success()
    {
        // Act
        var result = collection.Object.GetSortDefinition();

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void MongoRepositoryCollectionExtensions_GetUpdateDefinition_Success()
    {
        // Act
        var result = collection.Object.GetUpdateDefinition();

        // Assert
        result.Should().NotBeNull();
    }
}