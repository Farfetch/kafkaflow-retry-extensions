using System;
using FluentAssertions;
using global::KafkaFlow.Retry.Durable.Repository.Model;
using global::KafkaFlow.Retry.SqlServer.Model;
using global::KafkaFlow.Retry.SqlServer.Readers.Adapters;
using Xunit;

namespace KafkaFlow.Retry.UnitTests.Repositories.SqlServer.Readers.Adapters;

public class RetryQueueAdapterTests
{
    private readonly RetryQueueAdapter adapter = new RetryQueueAdapter();

    [Fact]
    public void RetryQueueAdapter_Adapt_Success()
    {
        // Arrange
        var retryQueue = new RetryQueueDbo
        {
            CreationDate = DateTime.UtcNow,
            IdDomain = Guid.NewGuid(),
            Id = 1,
            LastExecution = DateTime.UtcNow,
            QueueGroupKey = "QueueGroupKey",
            SearchGroupKey = "SearchGroupKey",
            Status = RetryQueueStatus.Active
        };

        // Act
        var result = adapter.Adapt(retryQueue);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType(typeof(RetryQueue));
    }

    [Fact]
    public void RetryQueueAdapter_Adapt_WithoutRetryQueueDbo_ThrowsException()
    {
        // Arrange
        RetryQueueDbo retryQueue = null;

        // Act
        Action act = () => adapter.Adapt(retryQueue);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}