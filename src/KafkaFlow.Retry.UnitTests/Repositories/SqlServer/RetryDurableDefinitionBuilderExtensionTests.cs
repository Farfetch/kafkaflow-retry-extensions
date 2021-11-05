namespace KafkaFlow.Retry.UnitTests.Repositories.SqlServer
{
    using System.Diagnostics.CodeAnalysis;
    using FluentAssertions;
    using global::KafkaFlow.Retry.SqlServer;
    using Xunit;

    [ExcludeFromCodeCoverage]
    public class RetryDurableDefinitionBuilderExtensionTests
    {
        [Fact]
        public void RetryDurableDefinitionBuilderExtension_WithSqlServerDataProvider_Success()
        {
            // Arrange
            var builder = new RetryDurableDefinitionBuilder();

            // Act
            var result = builder.WithSqlServerDataProvider("connectionString", "databaseName");

            // Arrange
            result.Should().NotBeNull();
        }
    }
}