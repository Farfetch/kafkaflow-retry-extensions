namespace KafkaFlow.Retry.UnitTests.Repositories.SqlServer
{
    using FluentAssertions;
    using global::KafkaFlow.Retry.SqlServer;
    using Moq;
    using Xunit;

    public class RetryDurableDefinitionBuilderExtensionTests
    {
        [Fact]
        public void RetryDurableDefinitionBuilderExtension_WithSqlServerDataProvider_Success()
        {
            // Arrange
            Mock<IDependencyConfigurator> iDependencyConfigurator = new Mock<IDependencyConfigurator>();
            var builder = new RetryDurableDefinitionBuilder(iDependencyConfigurator.Object);

            // Act
            var result = builder.WithSqlServerDataProvider("connectionString", "databaseName");

            // Arrange
            result.Should().NotBeNull();
        }
    }
}