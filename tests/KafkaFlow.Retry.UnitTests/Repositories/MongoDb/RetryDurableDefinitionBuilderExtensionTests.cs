using System;
using KafkaFlow.Retry.MongoDb;

namespace KafkaFlow.Retry.UnitTests.Repositories.MongoDb;

public class RetryDurableDefinitionBuilderExtensionTests
{
    [Fact]
    public void RetryDurableDefinitionBuilder_WithMongoDbDataProvider_ConnectionStringInvalid_ThrowsException()
    {
        // Arrange
        var builder = new RetryDurableDefinitionBuilder();

        // Act
        Action act = () => builder.WithMongoDbDataProvider(
            "connectionString",
            "databaseName",
            "mongoDbretryQueueCollectionName",
            "mongoDbretryQueueItemCollectionName"
        );

        // Assert
        act.Should().Throw<DataProviderCreationException>();
    }

    [Fact]
    public void RetryDurableDefinitionBuilder_WithMongoDbDataProvider_Success()
    {
        // Arrange
        var builder = new RetryDurableDefinitionBuilder();

        // Act
        var result = builder.WithMongoDbDataProvider(
            "mongodb://localhost:27017/KafkaFlowRetry?maxPoolSize=1000",
            "Test",
            "RetryQueueCollectionName",
            "RetryQueueItemCollectionName"
        );

        result.Should().NotBeNull();
        result.Should().BeOfType(typeof(RetryDurableDefinitionBuilder));
    }
}