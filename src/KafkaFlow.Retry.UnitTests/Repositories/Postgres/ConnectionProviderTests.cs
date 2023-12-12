using System;
using FluentAssertions;
using KafkaFlow.Retry.Postgres;
using Xunit;

namespace KafkaFlow.Retry.UnitTests.Repositories.Postgres;
    
public class ConnectionProviderTests
{
    private readonly ConnectionProvider provider = new ConnectionProvider();

    [Fact]
    public void ConnectionProvider_Create_Success()
    {
            // Act
            var result = provider.Create(new PostgresDbSettings("connectionString", "databaseName"));

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
            var result = provider.CreateWithinTransaction(new PostgresDbSettings("connectionString", "databaseName"));

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