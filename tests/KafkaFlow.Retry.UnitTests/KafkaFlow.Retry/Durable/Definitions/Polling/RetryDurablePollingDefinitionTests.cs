using System;
using KafkaFlow.Retry.Durable.Definitions.Polling;

namespace KafkaFlow.Retry.UnitTests.KafkaFlow.Retry.Durable.Definitions.Polling;

public class RetryDurablePollingDefinitionTests
{
    [Fact]
    public void RetryDurablePollingDefinition_Ctor_WithArgumentException_ThrowsException()
    {
        // Act
        Action act = () => new RetryDurablePollingDefinition(
            true,
            "",
            1,
            2);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(-1, 2)]
    [InlineData(1, -2)]
    public void RetryDurablePollingDefinition_Ctor_WithArgumentOutOfRangeException_ThrowsException(int fetchSize,
        int expirationIntervalFactor)
    {
        // Act
        Action act = () => new RetryDurablePollingDefinition(
            false,
            "x",
            fetchSize,
            expirationIntervalFactor);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}