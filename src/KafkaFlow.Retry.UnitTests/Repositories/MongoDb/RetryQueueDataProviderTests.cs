namespace KafkaFlow.Retry.UnitTests.Repositories.MongoDb
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using global::KafkaFlow.Retry.Durable.Repository.Actions.Read;
    using global::KafkaFlow.Retry.Durable.Repository.Model;
    using global::KafkaFlow.Retry.MongoDb;
    using global::KafkaFlow.Retry.MongoDb.Model;
    using global::KafkaFlow.Retry.MongoDb.Repositories;
    using MongoDB.Driver;
    using MongoDB.Driver.Linq;
    using Moq;
    using Xunit;

    public class RetryQueueDataProviderTests
    {
        private readonly Mock<IMongoCollection<RetryQueueDbo>> collection = new Mock<IMongoCollection<RetryQueueDbo>>();
        private readonly Mock<IMongoClient> mongoClient = new Mock<IMongoClient>();
        private readonly Mock<IMongoDatabase> mongoDatabase = new Mock<IMongoDatabase>();
        private readonly RetryQueueDataProvider provider;
        private readonly Mock<IMongoQueryable<RetryQueueDbo>> queryable = new Mock<IMongoQueryable<RetryQueueDbo>>();
        private readonly Mock<IRetryQueueRepository> repository = new Mock<IRetryQueueRepository>();
        private readonly Mock<IRetryQueueItemRepository> repositoryItem = new Mock<IRetryQueueItemRepository>();
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

        public RetryQueueDataProviderTests()
        {
            collection
                .Setup(d => d.AsQueryable(It.IsAny<IClientSessionHandle>(), It.IsAny<AggregateOptions>()))
                .Returns(queryable.Object);

            mongoDatabase.Setup(d => d.GetCollection<RetryQueueDbo>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings>()))
               .Returns(collection.Object);

            mongoClient.Setup(d => d.GetDatabase(It.IsAny<string>(), It.IsAny<MongoDatabaseSettings>()))
                .Returns(mongoDatabase.Object);

            var dbContext = new DbContext(new MongoDbSettings(), mongoClient.Object);

            provider = new RetryQueueDataProvider(
                dbContext,
                repository.Object,
                repositoryItem.Object);
        }

        [Fact(Skip = "Todo")]
        public async Task RetryQueueDataProvider_CheckQueueAsync_Success()
        {
            // Act
            var result = await provider.CheckQueueAsync(
                new CheckQueueInput(
                    new RetryQueueItemMessage("topicName", new byte[] { 1, 3 }, new byte[] { 2, 4, 6 }, 3, 21, DateTime.UtcNow),
                    "queueGroupKey"))
                .ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
        }

        [Fact(Skip = "Todo")]
        public async Task RetryQueueDataProvider_CheckQueueAsync_WithoutCheckQueueInput_ThrowsException()
        {
            // Act
            Action act = async () => await provider.CheckQueueAsync(null).ConfigureAwait(false);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact(Skip = "Todo")]
        public void RetryQueueDataProvider_Ctor_WithoutDbContext_ThrowsException()
        {
            // Act
            Action act = () => new RetryQueueDataProvider(null, Mock.Of<IRetryQueueRepository>(), Mock.Of<IRetryQueueItemRepository>());

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }
    }
}