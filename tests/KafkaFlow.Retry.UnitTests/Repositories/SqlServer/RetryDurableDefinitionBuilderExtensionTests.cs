using KafkaFlow.Retry.SqlServer;

namespace KafkaFlow.Retry.UnitTests.Repositories.SqlServer;

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

    [Fact]
    public void RetryDurableDefinitionBuilderExtension_WithSqlServerDataProviderAndSchema_Success()
    {
        // Arrange
        var builder = new RetryDurableDefinitionBuilder();

        // Act
        var result = builder.WithSqlServerDataProvider("connectionString", "databaseName", "schema");

        // Arrange
        result.Should().NotBeNull();
    }
}