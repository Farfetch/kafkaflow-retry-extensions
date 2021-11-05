namespace KafkaFlow.Retry.UnitTests.Repositories.SqlServer
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using FluentAssertions;
    using global::KafkaFlow.Retry.SqlServer;
    using Xunit;

    [ExcludeFromCodeCoverage]
    public class ConnectionProviderTests
    {
        private readonly ConnectionProvider provider = new ConnectionProvider();

        [Fact]
        public void ConnectionProvider_Create_Success()
        {
            // Act
            var result = provider.Create(new SqlServerDbSettings("connectionString", "databaseName"));

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
            var result = provider.CreateWithinTransaction(new SqlServerDbSettings("connectionString", "databaseName"));

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
}