using System;
using KafkaFlow.Retry.MongoDb;

namespace KafkaFlow.Retry.UnitTests.Repositories.MongoDb;

public class MongoDbDataProviderFactoryTests
{
    private readonly MongoDbDataProviderFactory _mongoDbDataProviderFactory = new();

    [Fact]
    public void MongoDbDataProviderFactory_TryCreate_ReturnsDataProviderCreationResult()
    {
        // Act
        var result = _mongoDbDataProviderFactory.TryCreate(new MongoDbSettings());

        // Arrange
        result.Should().NotBeNull();
        result.Should().BeOfType(typeof(DataProviderCreationResult));
    }

    [Fact]
    public void MongoDbDataProviderFactory_TryCreate_WithoutMongoDbSettings_ThrowsException()
    {
        // Act
        Action act = () => _mongoDbDataProviderFactory.TryCreate(null);

        // Arrange
        act.Should().Throw<ArgumentNullException>();
    }
}