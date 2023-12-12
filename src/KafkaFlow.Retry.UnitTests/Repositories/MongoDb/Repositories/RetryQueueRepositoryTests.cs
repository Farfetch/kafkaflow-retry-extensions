using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using global::KafkaFlow.Retry.Durable.Repository.Actions.Read;
using global::KafkaFlow.Retry.Durable.Repository.Model;
using global::KafkaFlow.Retry.MongoDb;
using global::KafkaFlow.Retry.MongoDb.Model;
using global::KafkaFlow.Retry.MongoDb.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace KafkaFlow.Retry.UnitTests.Repositories.MongoDb.Repositories;

public class RetryQueueRepositoryTests
{
    private readonly Mock<IMongoCollection<RetryQueueDbo>> collection = new Mock<IMongoCollection<RetryQueueDbo>>();

    private readonly Mock<IMongoClient> mongoClient = new Mock<IMongoClient>();
    private readonly Mock<IMongoDatabase> mongoDatabase = new Mock<IMongoDatabase>();
    private readonly RetryQueueRepository repository;
    private readonly Mock<IAsyncCursor<RetryQueueDbo>> retries = new Mock<IAsyncCursor<RetryQueueDbo>>();

    private readonly RetryQueueDbo retryQueueDbo = new RetryQueueDbo
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
        retries.SetupSequence(d => d.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);

        retries.Setup(d => d.Current).Returns(() => new List<RetryQueueDbo>
        {
            retryQueueDbo
        });

        collection
            .Setup(d => d.FindAsync(
                It.IsAny<FilterDefinition<RetryQueueDbo>>(),
                It.IsAny<FindOptions<RetryQueueDbo, RetryQueueDbo>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(retries.Object);

        collection
            .Setup(d => d.UpdateOneAsync(
                It.IsAny<FilterDefinition<RetryQueueDbo>>(),
                It.IsAny<UpdateDefinition<RetryQueueDbo>>(),
                It.IsAny<UpdateOptions>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(new UpdateResult.Acknowledged(1, 1, BsonBinaryData.Create(retryQueueDbo.Id)));

        mongoDatabase.Setup(d => d.GetCollection<RetryQueueDbo>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings>()))
            .Returns(collection.Object);

        mongoClient.Setup(d => d.GetDatabase(It.IsAny<string>(), It.IsAny<MongoDatabaseSettings>()))
            .Returns(mongoDatabase.Object);

        var dbContext = new DbContext(new MongoDbSettings(), mongoClient.Object);
        repository = new RetryQueueRepository(dbContext);
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
        var result = await repository
            .GetQueueAsync(retryQueueDbo.SearchGroupKey)
            .ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(retryQueueDbo);

        collection.Verify(d =>
                d.FindAsync(
                    It.IsAny<FilterDefinition<RetryQueueDbo>>(),
                    It.IsAny<FindOptions<RetryQueueDbo, RetryQueueDbo>>(),
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData(GetQueuesSortOption.ByCreationDate_Descending)]
    [InlineData(GetQueuesSortOption.ByLastExecution_Ascending)]
    public async Task RetryQueueRepository_GetTopSortedQueuesAsync_ByCreationDate_Descending_Success(GetQueuesSortOption getQueuesSortOption)
    {
        // Act
        var result = await repository
            .GetTopSortedQueuesAsync(
                RetryQueueStatus.Active,
                getQueuesSortOption,
                retryQueueDbo.SearchGroupKey,
                1)
            .ConfigureAwait(false);

        // Assert
        result.Should().NotBeEmpty();
        result.FirstOrDefault().Should().Be(retryQueueDbo);

        collection.Verify(d =>
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
        var result = await repository
            .UpdateLastExecutionAsync(retryQueueDbo.Id, DateTime.UtcNow)
            .ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.IsAcknowledged.Should().BeTrue();
        result.ModifiedCount.Should().Be(1);

        collection.Verify(d =>
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
        var result = await repository
            .UpdateStatusAndLastExecutionAsync(retryQueueDbo.Id, RetryQueueStatus.Done, DateTime.UtcNow)
            .ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.IsAcknowledged.Should().BeTrue();
        result.ModifiedCount.Should().Be(1);

        collection.Verify(d =>
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
        var result = await repository
            .UpdateStatusAsync(retryQueueDbo.Id, RetryQueueStatus.Done)
            .ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.IsAcknowledged.Should().BeTrue();
        result.ModifiedCount.Should().Be(1);

        collection.Verify(d =>
                d.UpdateOneAsync(
                    It.IsAny<FilterDefinition<RetryQueueDbo>>(),
                    It.IsAny<UpdateDefinition<RetryQueueDbo>>(),
                    It.IsAny<UpdateOptions>(),
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }
}