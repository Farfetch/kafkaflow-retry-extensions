using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KafkaFlow.Retry.Durable.Common;
using KafkaFlow.Retry.Durable.Repository.Actions.Update;
using KafkaFlow.Retry.Durable.Repository.Model;
using KafkaFlow.Retry.MongoDb;
using KafkaFlow.Retry.MongoDb.Model;
using KafkaFlow.Retry.MongoDb.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;

namespace KafkaFlow.Retry.UnitTests.Repositories.MongoDb.Repositories;

public class RetryQueueItemRepositoryTests
{
    private readonly Mock<IMongoCollection<RetryQueueItemDbo>> _collection = new Mock<IMongoCollection<RetryQueueItemDbo>>();

    private readonly Mock<IMongoClient> _mongoClient = new Mock<IMongoClient>();
    private readonly Mock<IMongoDatabase> _mongoDatabase = new Mock<IMongoDatabase>();
    private readonly RetryQueueItemRepository _repository;
    private readonly Mock<IAsyncCursor<RetryQueueItemDbo>> _retries = new Mock<IAsyncCursor<RetryQueueItemDbo>>();

    private readonly RetryQueueItemDbo _retryQueueItemDbo = new RetryQueueItemDbo
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
    };

    public RetryQueueItemRepositoryTests()
    {
        _retries.SetupSequence(d => d.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);

        _retries.Setup(d => d.Current).Returns(() => new List<RetryQueueItemDbo>
        {
            _retryQueueItemDbo
        });

        _collection
            .Setup(d => d.FindAsync(
                It.IsAny<FilterDefinition<RetryQueueItemDbo>>(),
                It.IsAny<FindOptions<RetryQueueItemDbo, RetryQueueItemDbo>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(_retries.Object);

        _mongoDatabase.Setup(d => d.GetCollection<RetryQueueItemDbo>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings>()))
            .Returns(_collection.Object);

        _mongoClient.Setup(d => d.GetDatabase(It.IsAny<string>(), It.IsAny<MongoDatabaseSettings>()))
            .Returns(_mongoDatabase.Object);

        var dbContext = new DbContext(new MongoDbSettings(), _mongoClient.Object);
        _repository = new RetryQueueItemRepository(dbContext);
    }

    [Fact]
    public async Task RetryQueueItemRepository_AnyItemStillActiveAsync_Success()
    {
        // Arrange
        var retryQueueId = Guid.NewGuid();

        // Act
        var result = await _repository
            .AnyItemStillActiveAsync(retryQueueId);

        // Assert
        result.Should().BeTrue();

        _collection.Verify(d =>
                d.FindAsync(
                    It.IsAny<FilterDefinition<RetryQueueItemDbo>>(),
                    It.IsAny<FindOptions<RetryQueueItemDbo, RetryQueueItemDbo>>(),
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public void RetryQueueItemRepository_Ctor_WithoutDbContext_ThrowException()
    {
        // Act
        Action act = () => new RetryQueueItemRepository(null);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task RetryQueueItemRepository_GetItemAsync_Success()
    {
        // Arrange
        var retryQueueId = Guid.NewGuid();

        // Act
        var result = await _repository
            .GetItemAsync(retryQueueId);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(_retryQueueItemDbo);
        _collection.Verify(d =>
                d.FindAsync(
                    It.IsAny<FilterDefinition<RetryQueueItemDbo>>(),
                    It.IsAny<FindOptions<RetryQueueItemDbo, RetryQueueItemDbo>>(),
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RetryQueueItemRepository_GetItemsAsync_Success()
    {
        // Arrange
        var retriesQueueId = new List<Guid>
        {
            _retryQueueItemDbo.Id
        };

        // Act
        var result = await _repository
            .GetItemsAsync(
                retriesQueueId,
                new List<RetryQueueItemStatus> { RetryQueueItemStatus.Waiting });

        // Assert
        result.Should().NotBeEmpty();
        result.FirstOrDefault().Should().Be(_retryQueueItemDbo);
        _collection.Verify(d =>
                d.FindAsync(
                    It.IsAny<FilterDefinition<RetryQueueItemDbo>>(),
                    It.IsAny<FindOptions<RetryQueueItemDbo, RetryQueueItemDbo>>(),
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RetryQueueItemRepository_IsFirstWaitingInQueue_Success()
    {
        // Act
        var result = await _repository
            .IsFirstWaitingInQueue(_retryQueueItemDbo);

        // Assert
        result.Should().BeTrue();
        _collection.Verify(d =>
                d.FindAsync(
                    It.IsAny<FilterDefinition<RetryQueueItemDbo>>(),
                    It.IsAny<FindOptions<RetryQueueItemDbo, RetryQueueItemDbo>>(),
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RetryQueueItemRepository_UpdateItemAsync_Success()
    {
        // Arrange
        _collection
            .Setup(d => d.UpdateOneAsync(
                It.IsAny<FilterDefinition<RetryQueueItemDbo>>(),
                It.IsAny<UpdateDefinition<RetryQueueItemDbo>>(),
                It.IsAny<UpdateOptions>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(new UpdateResult.Acknowledged(1, 1, BsonBinaryData.Create(_retryQueueItemDbo.Id)));

        // Act
        var result = await _repository
            .UpdateItemAsync(
                _retryQueueItemDbo.Id,
                RetryQueueItemStatus.Done,
                1,
                DateTime.UtcNow,
                "description");

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(_retryQueueItemDbo.Id);
        result.Status.Should().Be(UpdateItemResultStatus.Updated);

        _collection.Verify(d =>
                d.UpdateOneAsync(
                    It.IsAny<FilterDefinition<RetryQueueItemDbo>>(),
                    It.IsAny<UpdateDefinition<RetryQueueItemDbo>>(),
                    It.IsAny<UpdateOptions>(),
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RetryQueueItemRepository_UpdateItemAsyncItemNotFound_Success()
    {
        // Arrange
        _collection
            .Setup(d => d.UpdateOneAsync(
                It.IsAny<FilterDefinition<RetryQueueItemDbo>>(),
                It.IsAny<UpdateDefinition<RetryQueueItemDbo>>(),
                It.IsAny<UpdateOptions>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(new UpdateResult.Acknowledged(0, null, BsonBinaryData.Create(_retryQueueItemDbo.Id)));

        // Act
        var result = await _repository
            .UpdateItemAsync(
                _retryQueueItemDbo.Id,
                RetryQueueItemStatus.Done,
                1,
                DateTime.UtcNow,
                "description");

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(_retryQueueItemDbo.Id);
        result.Status.Should().Be(UpdateItemResultStatus.ItemNotFound);

        _collection.Verify(d =>
                d.UpdateOneAsync(
                    It.IsAny<FilterDefinition<RetryQueueItemDbo>>(),
                    It.IsAny<UpdateDefinition<RetryQueueItemDbo>>(),
                    It.IsAny<UpdateOptions>(),
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }
}