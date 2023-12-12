using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KafkaFlow.Retry.Durable.Common;
using KafkaFlow.Retry.Durable.Repository.Model;
using KafkaFlow.Retry.MongoDb;
using KafkaFlow.Retry.MongoDb.Model;
using MongoDB.Driver;
using Moq;

namespace KafkaFlow.Retry.UnitTests.Repositories.MongoDb;

public class MongoRepositoryCollectionExtensionsTests
{
    private readonly Mock<IMongoCollection<RetryQueueItemDbo>> _collection = new Mock<IMongoCollection<RetryQueueItemDbo>>();
    private readonly Mock<IAsyncCursor<RetryQueueItemDbo>> _retries = new Mock<IAsyncCursor<RetryQueueItemDbo>>();

    private readonly IEnumerable<RetryQueueItemDbo> _retryQueueItemDbos = new List<RetryQueueItemDbo>
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
        _retries.SetupSequence(d => d.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);

        _retries.Setup(d => d.Current).Returns(() => _retryQueueItemDbos);

        _collection
            .Setup(d => d.FindAsync(
                It.IsAny<FilterDefinition<RetryQueueItemDbo>>(),
                It.IsAny<FindOptions<RetryQueueItemDbo, RetryQueueItemDbo>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(_retries.Object);
    }

    [Fact]
    public async Task MongoRepositoryCollectionExtensions_GetAsync_Success()
    {
        // Arrange
        var filter = FilterDefinition<RetryQueueItemDbo>.Empty;

        // Act
        var result = await _collection.Object.GetAsync(filter);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().BeEquivalentTo(_retryQueueItemDbos);
        _collection.Verify(d =>
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
        var result = _collection.Object.GetFilters();

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task MongoRepositoryCollectionExtensions_GetOneAsync_Success()
    {
        // Arrange
        var filter = FilterDefinition<RetryQueueItemDbo>.Empty;

        // Act
        var result = await _collection.Object.GetOneAsync(filter);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(_retryQueueItemDbos.FirstOrDefault());
        _collection.Verify(d =>
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
        var result = _collection.Object.GetSortDefinition();

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void MongoRepositoryCollectionExtensions_GetUpdateDefinition_Success()
    {
        // Act
        var result = _collection.Object.GetUpdateDefinition();

        // Assert
        result.Should().NotBeNull();
    }
}