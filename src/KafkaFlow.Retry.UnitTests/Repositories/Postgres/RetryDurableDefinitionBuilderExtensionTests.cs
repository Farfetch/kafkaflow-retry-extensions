namespace KafkaFlow.Retry.UnitTests.Repositories.Postgres
{
    using FluentAssertions;
    using global::KafkaFlow.Retry.Postgres;
    using Xunit;
    
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