using System;
using KafkaFlow.Retry.SqlServer;

namespace KafkaFlow.Retry.UnitTests.Repositories.SqlServer;

public class ConnectionProviderTests
{
    private readonly ConnectionProvider provider = new ConnectionProvider();

    [Fact]
    public void ConnectionProvider_Create_Success()
    {
            // Act
            var result = provider.Create(new SqlServerDbSettings("connectionString", "databaseName", "schema"));

            // Arrange
            result.Should().NotBeNull();
            result.Should().BeOfType(typeof(DbConnectionContext));
        }

    [Fact]
    public void ConnectionProvider_Create_WithoutSqlServerDbSettings_ThrowsException()
    {
            // Act
            Action act = () => provider.Create(null);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

    [Fact]
    public void ConnectionProvider_CreateWithinTransaction_Success()
    {
            // Act
            var result = provider.CreateWithinTransaction(new SqlServerDbSettings("connectionString", "databaseName", "schema"));

            // Arrange
            result.Should().NotBeNull();
            result.Should().BeOfType(typeof(DbConnectionContext));
        }

    [Fact]
    public void ConnectionProvider_CreateWithinTransaction_WithoutSqlServerDbSettings_ThrowsException()
    {
            // Act
            Action act = () => provider.CreateWithinTransaction(null);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }
}