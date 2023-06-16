namespace KafkaFlow.Retry.UnitTests.KafkaFlow.Retry.Durable.Definitions
{
    using System;
    using FluentAssertions;
    using global::KafkaFlow.Retry.Durable.Definitions;
    using Xunit;

    public class RetryDurablePollingDefinitionTests
    {
        [Fact]
        public void RetryDurablePollingDefinition__Ctor_WithArgumentNullException_ThrowsException()
        {
            // Act
            Action act = () => new RetryDurablePollingDefinition(
                enabled: false,
                cronExpression: "",
                fetchSize: 1,
                expirationIntervalFactor: 2,
                id: null);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Theory]
        [InlineData(-1, 2)]
        [InlineData(1, -2)]
        public void RetryDurablePollingDefinition__Ctor_WithArgumentOutOfRangeException_ThrowsException(int fetchSize, int expirationIntervalFactor)
        {
            // Act
            Action act = () => new RetryDurablePollingDefinition(
                enabled: false,
                cronExpression: "x",
                fetchSize,
                expirationIntervalFactor,
                id: "id");

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void RetryDurablePollingDefinition_Ctor_WithArgumentException_ThrowsException()
        {
            // Act
            Action act = () => new RetryDurablePollingDefinition(
                enabled: true,
                cronExpression: "",
                fetchSize: 1,
                expirationIntervalFactor: 2,
                id: "id");

            // Assert
            act.Should().Throw<ArgumentException>();
        }
    }
}