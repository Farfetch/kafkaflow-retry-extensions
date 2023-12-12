using KafkaFlow.Retry.MongoDb;
using KafkaFlow.Retry.MongoDb.Model;
using MongoDB.Driver;
using Moq;

namespace KafkaFlow.Retry.UnitTests.Repositories.MongoDb;

public class DbContextTests
{
    private readonly Mock<IMongoCollection<RetryQueueDbo>> _collectionRetryQueueDbo = new Mock<IMongoCollection<RetryQueueDbo>>();
    private readonly Mock<IMongoCollection<RetryQueueItemDbo>> _collectionRetryQueueItemDbo = new Mock<IMongoCollection<RetryQueueItemDbo>>();
    private readonly DbContext _dbContext;
    private readonly Mock<IMongoClient> _mongoClient = new Mock<IMongoClient>();
    private readonly Mock<IMongoDatabase> _mongoDatabase = new Mock<IMongoDatabase>();

    public DbContextTests()
    {
        _mongoDatabase.Setup(d => d.GetCollection<RetryQueueItemDbo>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings>()))
            .Returns(_collectionRetryQueueItemDbo.Object);

        _mongoDatabase.Setup(d => d.GetCollection<RetryQueueDbo>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings>()))
            .Returns(_collectionRetryQueueDbo.Object);

        _mongoClient.Setup(d => d.GetDatabase(It.IsAny<string>(), It.IsAny<MongoDatabaseSettings>()))
            .Returns(_mongoDatabase.Object);

        _dbContext = new DbContext(new MongoDbSettings(), _mongoClient.Object);
    }

    [Fact]
    public void DbContext_MongoClient_Success()
    {
        // Act
        var result = _dbContext.MongoClient;

        // Assert
        result.Should().Be(_mongoClient.Object);
    }

    [Fact]
    public void DbContext_RetryQueueItems_Success()
    {
        // Act
        _ = _dbContext.RetryQueueItems;

        // Assert
        _mongoDatabase
            .Verify(d => d.GetCollection<RetryQueueItemDbo>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings>()), Times.Once);
    }

    [Fact]
    public void DbContext_RetryQueues_Success()
    {
        // Act
        _ = _dbContext.RetryQueues;

        // Assert
        _mongoDatabase
            .Verify(d => d.GetCollection<RetryQueueDbo>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings>()), Times.Once);
    }
}