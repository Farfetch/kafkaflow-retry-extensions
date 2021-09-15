namespace KafkaFlow.Retry.UnitTests.Repositories.MongoDb
{
    using System;
    using FluentAssertions;
    using global::KafkaFlow.Retry.MongoDb;
    using Moq;
    using Xunit;

    public class RetryDurableDefinitionBuilderExtensionTests
    {
        [Fact]
        public void RetryDurableDefinitionBuilder_WithMongoDbDataProvider_ConnectionStringInvalid_ThrowsException()
        {
            // Arrange
            var iDependencyConfigurator = new Mock<IDependencyConfigurator>();
            var builder = new RetryDurableDefinitionBuilder(iDependencyConfigurator.Object);

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

        [Fact(Skip = "Todo")]
        public void RetryDurableDefinitionBuilder_WithMongoDbDataProvider_Success()
        {
            // Arrange
            var iDependencyConfigurator = new Mock<IDependencyConfigurator>();
            var builder = new RetryDurableDefinitionBuilder(iDependencyConfigurator.Object);

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
}