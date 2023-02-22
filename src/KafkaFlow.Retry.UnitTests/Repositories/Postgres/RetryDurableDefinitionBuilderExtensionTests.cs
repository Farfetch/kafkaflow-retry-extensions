using FluentAssertions;
using KafkaFlow.Retry.Postgres;
using Xunit;

namespace KafkaFlow.Retry.UnitTests.Repositories.Postgres
{
    public class RetryDurableDefinitionBuilderExtensionTests
    {
        [Fact]
        public void RetryDurableDefinitionBuilderExtension_WithSqlServerDataProvider_Success()
        {
            // Arrange
            var builder = new RetryDurableDefinitionBuilder();

            // Act
            var result = builder.WithPostgresDataProvider("connectionString", "databaseName");

            // Arrange
            result.Should().NotBeNull();
        }
    }
}