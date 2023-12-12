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
    private readonly Mock<IMongoCollection<RetryQueueItemDbo>> collection = new Mock<IMongoCollection<RetryQueueItemDbo>>();

    private readonly Mock<IMongoClient> mongoClient = new Mock<IMongoClient>();
    private readonly Mock<IMongoDatabase> mongoDatabase = new Mock<IMongoDatabase>();
    private readonly RetryQueueItemRepository repository;
    private readonly Mock<IAsyncCursor<RetryQueueItemDbo>> retries = new Mock<IAsyncCursor<RetryQueueItemDbo>>();

    private readonly RetryQueueItemDbo retryQueueItemDbo = new RetryQueueItemDbo
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
        retries.SetupSequence(d => d.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);

        retries.Setup(d => d.Current).Returns(() => new List<RetryQueueItemDbo>
        {
            retryQueueItemDbo
        });

        collection
            .Setup(d => d.FindAsync(
                It.IsAny<FilterDefinition<RetryQueueItemDbo>>(),
                It.IsAny<FindOptions<RetryQueueItemDbo, RetryQueueItemDbo>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(retries.Object);

        mongoDatabase.Setup(d => d.GetCollection<RetryQueueItemDbo>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings>()))
            .Returns(collection.Object);

        mongoClient.Setup(d => d.GetDatabase(It.IsAny<string>(), It.IsAny<MongoDatabaseSettings>()))
            .Returns(mongoDatabase.Object);

        var dbContext = new DbContext(new MongoDbSettings(), mongoClient.Object);
        repository = new RetryQueueItemRepository(dbContext);
    }

    [Fact]
    public async Task RetryQueueItemRepository_AnyItemStillActiveAsync_Success()
    {
        // Arrange
        var retryQueueId = Guid.NewGuid();

        // Act
        var result = await repository
            .AnyItemStillActiveAsync(retryQueueId)
            .ConfigureAwait(false);

        // Assert
        result.Should().BeTrue();

        collection.Verify(d =>
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
        var result = await repository
            .GetItemAsync(retryQueueId)
            .ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(retryQueueItemDbo);
        collection.Verify(d =>
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
            retryQueueItemDbo.Id
        };

        // Act
        var result = await repository
            .GetItemsAsync(
                retriesQueueId,
                new List<RetryQueueItemStatus> { RetryQueueItemStatus.Waiting })
            .ConfigureAwait(false);

        // Assert
        result.Should().NotBeEmpty();
        result.FirstOrDefault().Should().Be(retryQueueItemDbo);
        collection.Verify(d =>
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
        var result = await repository
            .IsFirstWaitingInQueue(retryQueueItemDbo)
            .ConfigureAwait(false);

        // Assert
        result.Should().BeTrue();
        collection.Verify(d =>
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
        collection
            .Setup(d => d.UpdateOneAsync(
                It.IsAny<FilterDefinition<RetryQueueItemDbo>>(),
                It.IsAny<UpdateDefinition<RetryQueueItemDbo>>(),
                It.IsAny<UpdateOptions>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(new UpdateResult.Acknowledged(1, 1, BsonBinaryData.Create(retryQueueItemDbo.Id)));

        // Act
        var result = await repository
            .UpdateItemAsync(
                retryQueueItemDbo.Id,
                RetryQueueItemStatus.Done,
                1,
                DateTime.UtcNow,
                "description")
            .ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(retryQueueItemDbo.Id);
        result.Status.Should().Be(UpdateItemResultStatus.Updated);

        collection.Verify(d =>
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
        collection
            .Setup(d => d.UpdateOneAsync(
                It.IsAny<FilterDefinition<RetryQueueItemDbo>>(),
                It.IsAny<UpdateDefinition<RetryQueueItemDbo>>(),
                It.IsAny<UpdateOptions>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(new UpdateResult.Acknowledged(0, null, BsonBinaryData.Create(retryQueueItemDbo.Id)));

        // Act
        var result = await repository
            .UpdateItemAsync(
                retryQueueItemDbo.Id,
                RetryQueueItemStatus.Done,
                1,
                DateTime.UtcNow,
                "description")
            .ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(retryQueueItemDbo.Id);
        result.Status.Should().Be(UpdateItemResultStatus.ItemNotFound);

        collection.Verify(d =>
                d.UpdateOneAsync(
                    It.IsAny<FilterDefinition<RetryQueueItemDbo>>(),
                    It.IsAny<UpdateDefinition<RetryQueueItemDbo>>(),
                    It.IsAny<UpdateOptions>(),
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }
}