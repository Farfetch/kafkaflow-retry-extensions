namespace KafkaFlow.Retry.UnitTests.Repositories.MongoDb
{
    using System;
    using FluentAssertions;
    using global::KafkaFlow.Retry.MongoDb;
    using Xunit;

    public class MongoDbDataProviderFactoryTests
    {
        private readonly MongoDbDataProviderFactory mongoDbDataProviderFactory = new MongoDbDataProviderFactory();

        [Fact]
        public void MongoDbDataProviderFactory_TryCreate_ReturnsDataProviderCreationResult()
        {
            // Act
            var result = mongoDbDataProviderFactory.TryCreate(new MongoDbSettings());

            // Arrange
            result.Should().NotBeNull();
            result.Should().BeOfType(typeof(DataProviderCreationResult));
        }

        [Fact]
        public void MongoDbDataProviderFactory_TryCreate_WithoutMongoDbSettings_ThrowsException()
        {
            // Act
            Action act = () => mongoDbDataProviderFactory.TryCreate(null);

            // Arrange
            act.Should().Throw<ArgumentNullException>();
        }
    }
}