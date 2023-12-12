using System;
using KafkaFlow.Retry.Durable.Repository.Model;
using KafkaFlow.Retry.SqlServer.Model;
using KafkaFlow.Retry.SqlServer.Readers.Adapters;

namespace KafkaFlow.Retry.UnitTests.Repositories.SqlServer.Readers.Adapters;

public class RetryQueueAdapterTests
{
    private readonly RetryQueueAdapter _adapter = new RetryQueueAdapter();

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
        var result = _adapter.Adapt(retryQueue);

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
        Action act = () => _adapter.Adapt(retryQueue);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}