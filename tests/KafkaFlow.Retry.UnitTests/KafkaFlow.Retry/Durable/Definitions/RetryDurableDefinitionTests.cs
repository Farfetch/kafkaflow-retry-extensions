using System;
using System.Collections.Generic;
using KafkaFlow.Retry.Durable.Definitions;
using KafkaFlow.Retry.Durable.Repository;
using Moq;

namespace KafkaFlow.Retry.UnitTests.KafkaFlow.Retry.Durable.Definitions;

public class RetryDurableDefinitionTests
{
    private static readonly RetryContext s_retry = new(new Exception());
    private readonly Mock<IRetryDurableQueueRepository> _retryDurableQueueRepository = new();

    private readonly RetryDurableRetryPlanBeforeDefinition _retryDurableRetryPlanBeforeDefinition =
        new(_ => new TimeSpan(1), 1, false);

    private readonly IReadOnlyCollection<Func<RetryContext, bool>> _retryWhenExceptions =
        new List<Func<RetryContext, bool>>
        {
            d => d.Exception is null
        };

    [Fact]
    public void RetryDurableDefinition_Ctor_WithNullArgsForRetryDurablePollingDefinition_ThrowsException()
    {
        // Act
        Action act = () =>
            new RetryDurableDefinition(_retryWhenExceptions, _retryDurableRetryPlanBeforeDefinition, null);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RetryDurableDefinition_Ctor_WithNullArgsForRetryDurableRetryPlanBeforeDefinition_ThrowsException()
    {
        // Act
        Action act = () => new RetryDurableDefinition(_retryWhenExceptions, null, _retryDurableQueueRepository.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RetryDurableDefinition_Ctor_WithNullArgsForRetryWhenExceptions_ThrowsException()
    {
        // Act

        Action act = () =>
            new RetryDurableDefinition(null, _retryDurableRetryPlanBeforeDefinition,
                _retryDurableQueueRepository.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RetryDurableDefinition_ShouldRetry_Success()
    {
        // Arrange
        var retryDurableDefinition = new RetryDurableDefinition(
            _retryWhenExceptions,
            _retryDurableRetryPlanBeforeDefinition,
            _retryDurableQueueRepository.Object);

        // Act
        var result = retryDurableDefinition.ShouldRetry(s_retry);

        // Arrange
        result.Should().BeFalse();
    }
}