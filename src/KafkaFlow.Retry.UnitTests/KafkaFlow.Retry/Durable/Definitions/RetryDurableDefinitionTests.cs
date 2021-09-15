namespace KafkaFlow.Retry.UnitTests.KafkaFlow.Retry.Durable.Definitions
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using global::KafkaFlow.Retry.Durable.Definitions;
    using Xunit;

    public class RetryDurableDefinitionTests
    {
        private static readonly RetryContext retry = new RetryContext(new Exception());
        private readonly IRetryDurablePollingDefinition retryDurablePollingDefinition = new RetryDurablePollingDefinition(false, "ad", 1, 1, "ad");
        private readonly IRetryDurableRetryPlanBeforeDefinition retryDurableRetryPlanBeforeDefinition = new RetryDurableRetryPlanBeforeDefinition(new Func<int, TimeSpan>(_ => new TimeSpan(1)), 1, false);

        private readonly IReadOnlyCollection<Func<RetryContext, bool>> retryWhenExceptions = new List<Func<RetryContext, bool>>
        {
            new Func<RetryContext, bool>(d=> d.Exception is null)
        };

        [Fact]
        public void RetryDurableDefinition_Ctor_WithNullArgsForRetryDurablePollingDefinition_ThrowsException()
        {
            // Act
            Action act = () => new RetryDurableDefinition(retryWhenExceptions, retryDurableRetryPlanBeforeDefinition, retryDurablePollingDefinition: null);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void RetryDurableDefinition_Ctor_WithNullArgsForRetryDurableRetryPlanBeforeDefinition_ThrowsException()
        {
            // Act
            Action act = () => new RetryDurableDefinition(retryWhenExceptions, retryDurableRetryPlanBeforeDefinition: null, retryDurablePollingDefinition);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void RetryDurableDefinition_Ctor_WithNullArgsForRetryWhenExceptions_ThrowsException()
        {
            // Act

            Action act = () => new RetryDurableDefinition(retryWhenExceptions: null, retryDurableRetryPlanBeforeDefinition, retryDurablePollingDefinition);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void RetryDurableDefinition_ShouldRetry_Success()
        {
            // Arrange
            var retryDurableDefinition = new RetryDurableDefinition(
                retryWhenExceptions,
                retryDurableRetryPlanBeforeDefinition,
                retryDurablePollingDefinition);

            // Act
            var result = retryDurableDefinition.ShouldRetry(retry);

            // Arrange
            result.Should().BeFalse();
        }
    }
}