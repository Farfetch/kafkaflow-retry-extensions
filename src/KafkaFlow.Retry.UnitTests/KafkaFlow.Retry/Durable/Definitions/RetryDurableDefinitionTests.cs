namespace KafkaFlow.Retry.UnitTests.KafkaFlow.Retry.Durable.Definitions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using FluentAssertions;
    using global::KafkaFlow.Retry.Durable.Definitions;
    using global::KafkaFlow.Retry.Durable.Repository;
    using Moq;
    using Xunit;

    [ExcludeFromCodeCoverage]
    public class RetryDurableDefinitionTests
    {
        private static readonly RetryContext retry = new RetryContext(new Exception());
        private readonly Mock<IRetryDurableQueueRepository> retryDurableQueueRepository = new Mock<IRetryDurableQueueRepository>();
        private readonly RetryDurableRetryPlanBeforeDefinition retryDurableRetryPlanBeforeDefinition = new RetryDurableRetryPlanBeforeDefinition(new Func<int, TimeSpan>(_ => new TimeSpan(1)), 1, false);

        private readonly IReadOnlyCollection<Func<RetryContext, bool>> retryWhenExceptions = new List<Func<RetryContext, bool>>
        {
            new Func<RetryContext, bool>(d=> d.Exception is null)
        };

        [Fact]
        public void RetryDurableDefinition_Ctor_WithNullArgsForRetryDurablePollingDefinition_ThrowsException()
        {
            // Act
            Action act = () => new RetryDurableDefinition(retryWhenExceptions, retryDurableRetryPlanBeforeDefinition, retryDurableQueueRepository: null);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void RetryDurableDefinition_Ctor_WithNullArgsForRetryDurableRetryPlanBeforeDefinition_ThrowsException()
        {
            // Act
            Action act = () => new RetryDurableDefinition(retryWhenExceptions, retryDurableRetryPlanBeforeDefinition: null, retryDurableQueueRepository.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void RetryDurableDefinition_Ctor_WithNullArgsForRetryWhenExceptions_ThrowsException()
        {
            // Act

            Action act = () => new RetryDurableDefinition(retryWhenExceptions: null, retryDurableRetryPlanBeforeDefinition, retryDurableQueueRepository.Object);

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
                retryDurableQueueRepository.Object);

            // Act
            var result = retryDurableDefinition.ShouldRetry(retry);

            // Arrange
            result.Should().BeFalse();
        }
    }
}