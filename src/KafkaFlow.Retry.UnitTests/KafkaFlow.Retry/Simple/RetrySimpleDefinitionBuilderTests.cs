namespace KafkaFlow.Retry.UnitTests.KafkaFlow.Retry.Simple
{
    using System;
    using FluentAssertions;
    using global::KafkaFlow.Retry.Simple;
    using Xunit;

    public class RetrySimpleDefinitionBuilderTests
    {
        [Fact]
        public void RetrySimpleDefinitionBuilder_Build_ReturnsRetryForeverDefinition()
        {
            // Arrange
            var builder = new RetrySimpleDefinitionBuilder();
            var exception = new Exception();
            var retryContext = new RetryContext(exception);
            var tryTimes = 1;
            var pause = false;

            builder.WithTimeBetweenTriesPlan(new TimeSpan[0])
            .TryTimes(tryTimes)
            .ShouldPauseConsumer(pause)
            .WithTimeBetweenTriesPlan(new Func<int, TimeSpan>(_ => new TimeSpan()))
            .Handle<Exception>()
            .Handle(new Func<Exception, bool>(_ => 1 == 1))
            .Handle(new Func<RetryContext, bool>((d) => d == retryContext));

            // Act
            var result = builder.Build();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType(typeof(RetrySimpleDefinition));
            result.ShouldRetry(retryContext).Should().BeTrue();
            result.TimeBetweenTriesPlan.Should().NotBeNull();
            result.NumberOfRetries.Should().Be(tryTimes);
            result.PauseConsumer.Should().Be(pause);
        }
    }
}