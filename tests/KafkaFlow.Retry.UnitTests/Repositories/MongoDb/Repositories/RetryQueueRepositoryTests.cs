using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KafkaFlow.Retry.Durable.Repository.Actions.Read;
using KafkaFlow.Retry.Durable.Repository.Model;
using KafkaFlow.Retry.MongoDb;
using KafkaFlow.Retry.MongoDb.Model;
using KafkaFlow.Retry.MongoDb.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;

namespace KafkaFlow.Retry.UnitTests.Repositories.MongoDb.Repositories;

public class RetryQueueRepositoryTests
{
    private readonly Mock<IMongoCollection<RetryQueueDbo>> _collection = new Mock<IMongoCollection<RetryQueueDbo>>();

    private readonly Mock<IMongoClient> _mongoClient = new Mock<IMongoClient>();
    private readonly Mock<IMongoDatabase> _mongoDatabase = new Mock<IMongoDatabase>();
    private readonly RetryQueueRepository _repository;
    private readonly Mock<IAsyncCursor<RetryQueueDbo>> _retries = new Mock<IAsyncCursor<RetryQueueDbo>>();

    private readonly RetryQueueDbo _retryQueueDbo = new RetryQueueDbo
    {
        Id = Guid.NewGuid(),
        CreationDate = DateTime.UtcNow,
        LastExecution = DateTime.UtcNow,
        Status = RetryQueueStatus.Active,
        QueueGroupKey = "1",
        SearchGroupKey = "1"
    };

    public RetryQueueRepositoryTests()
    {
        _retries.SetupSequence(d => d.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);

        _retries.Setup(d => d.Current).Returns(() => new List<RetryQueueDbo>
        {
            _retryQueueDbo
        });

        _collection
            .Setup(d => d.FindAsync(
                It.IsAny<FilterDefinition<RetryQueueDbo>>(),
                It.IsAny<FindOptions<RetryQueueDbo, RetryQueueDbo>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(_retries.Object);

        _collection
            .Setup(d => d.UpdateOneAsync(
                It.IsAny<FilterDefinition<RetryQueueDbo>>(),
                It.IsAny<UpdateDefinition<RetryQueueDbo>>(),
                It.IsAny<UpdateOptions>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(new UpdateResult.Acknowledged(1, 1, BsonBinaryData.Create(_retryQueueDbo.Id)));

        _mongoDatabase.Setup(d => d.GetCollection<RetryQueueDbo>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings>()))
            .Returns(_collection.Object);

        _mongoClient.Setup(d => d.GetDatabase(It.IsAny<string>(), It.IsAny<MongoDatabaseSettings>()))
            .Returns(_mongoDatabase.Object);

        var dbContext = new DbContext(new MongoDbSettings(), _mongoClient.Object);
        _repository = new RetryQueueRepository(dbContext);
    }

    [Fact]
    public void RetryQueueRepository_Ctor_WithoutDbContext_ThrowException()
    {
        // Act
        Action act = () => new RetryQueueRepository(null);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task RetryQueueRepository_GetQueueAsync_Success()
    {
        // Act
        var result = await _repository
            .GetQueueAsync(_retryQueueDbo.SearchGroupKey);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(_retryQueueDbo);

        _collection.Verify(d =>
                d.FindAsync(
                    It.IsAny<FilterDefinition<RetryQueueDbo>>(),
                    It.IsAny<FindOptions<RetryQueueDbo, RetryQueueDbo>>(),
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData(GetQueuesSortOption.ByCreationDateDescending)]
    [InlineData(GetQueuesSortOption.ByLastExecutionAscending)]
    public async Task RetryQueueRepository_GetTopSortedQueuesAsync_ByCreationDate_Descending_Success(GetQueuesSortOption getQueuesSortOption)
    {
        // Act
        var result = await _repository
            .GetTopSortedQueuesAsync(
                RetryQueueStatus.Active,
                getQueuesSortOption,
                _retryQueueDbo.SearchGroupKey,
                1);

        // Assert
        result.Should().NotBeEmpty();
        result.FirstOrDefault().Should().Be(_retryQueueDbo);

        _collection.Verify(d =>
                d.FindAsync(
                    It.IsAny<FilterDefinition<RetryQueueDbo>>(),
                    It.IsAny<FindOptions<RetryQueueDbo, RetryQueueDbo>>(),
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RetryQueueRepository_UpdateLastExecutionAsync_Success()
    {
        // Act
        var result = await _repository
            .UpdateLastExecutionAsync(_retryQueueDbo.Id, DateTime.UtcNow);

        // Assert
        result.Should().NotBeNull();
        result.IsAcknowledged.Should().BeTrue();
        result.ModifiedCount.Should().Be(1);

        _collection.Verify(d =>
                d.UpdateOneAsync(
                    It.IsAny<FilterDefinition<RetryQueueDbo>>(),
                    It.IsAny<UpdateDefinition<RetryQueueDbo>>(),
                    It.IsAny<UpdateOptions>(),
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RetryQueueRepository_UpdateStatusAndLastExecutionAsync_Success()
    {
        // Act
        var result = await _repository
            .UpdateStatusAndLastExecutionAsync(_retryQueueDbo.Id, RetryQueueStatus.Done, DateTime.UtcNow);

        // Assert
        result.Should().NotBeNull();
        result.IsAcknowledged.Should().BeTrue();
        result.ModifiedCount.Should().Be(1);

        _collection.Verify(d =>
                d.UpdateOneAsync(
                    It.IsAny<FilterDefinition<RetryQueueDbo>>(),
                    It.IsAny<UpdateDefinition<RetryQueueDbo>>(),
                    It.IsAny<UpdateOptions>(),
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RetryQueueRepository_UpdateStatusAsync_Success()
    {
        // Act
        var result = await _repository
            .UpdateStatusAsync(_retryQueueDbo.Id, RetryQueueStatus.Done);

        // Assert
        result.Should().NotBeNull();
        result.IsAcknowledged.Should().BeTrue();
        result.ModifiedCount.Should().Be(1);

        _collection.Verify(d =>
                d.UpdateOneAsync(
                    It.IsAny<FilterDefinition<RetryQueueDbo>>(),
                    It.IsAny<UpdateDefinition<RetryQueueDbo>>(),
                    It.IsAny<UpdateOptions>(),
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }
}