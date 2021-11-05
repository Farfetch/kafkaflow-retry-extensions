namespace KafkaFlow.Retry.UnitTests.Repositories.MongoDb
{
    using System.Diagnostics.CodeAnalysis;
    using FluentAssertions;
    using global::KafkaFlow.Retry.MongoDb;
    using global::KafkaFlow.Retry.MongoDb.Model;
    using MongoDB.Driver;
    using Moq;
    using Xunit;

    [ExcludeFromCodeCoverage]
    public class DbContextTests
    {
        private readonly Mock<IMongoCollection<RetryQueueDbo>> collectionRetryQueueDbo = new Mock<IMongoCollection<RetryQueueDbo>>();
        private readonly Mock<IMongoCollection<RetryQueueItemDbo>> collectionRetryQueueItemDbo = new Mock<IMongoCollection<RetryQueueItemDbo>>();
        private readonly DbContext dbContext;
        private readonly Mock<IMongoClient> mongoClient = new Mock<IMongoClient>();
        private readonly Mock<IMongoDatabase> mongoDatabase = new Mock<IMongoDatabase>();

        public DbContextTests()
        {
            mongoDatabase.Setup(d => d.GetCollection<RetryQueueItemDbo>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings>()))
                .Returns(collectionRetryQueueItemDbo.Object);

            mongoDatabase.Setup(d => d.GetCollection<RetryQueueDbo>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings>()))
                .Returns(collectionRetryQueueDbo.Object);

            mongoClient.Setup(d => d.GetDatabase(It.IsAny<string>(), It.IsAny<MongoDatabaseSettings>()))
                .Returns(mongoDatabase.Object);

            dbContext = new DbContext(new MongoDbSettings(), mongoClient.Object);
        }

        [Fact]
        public void DbContext_MongoClient_Success()
        {
            // Act
            var result = dbContext.MongoClient;

            // Assert
            result.Should().Be(mongoClient.Object);
        }

        [Fact]
        public void DbContext_RetryQueueItems_Success()
        {
            // Act
            _ = dbContext.RetryQueueItems;

            // Assert
            mongoDatabase
                .Verify(d => d.GetCollection<RetryQueueItemDbo>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings>()), Times.Once);
        }

        [Fact]
        public void DbContext_RetryQueues_Success()
        {
            // Act
            _ = dbContext.RetryQueues;

            // Assert
            mongoDatabase
                .Verify(d => d.GetCollection<RetryQueueDbo>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings>()), Times.Once);
        }
    }
}