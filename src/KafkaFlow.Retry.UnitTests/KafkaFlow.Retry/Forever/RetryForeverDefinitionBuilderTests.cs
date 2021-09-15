namespace KafkaFlow.Retry.UnitTests.KafkaFlow.Retry.Forever
{
    using System;
    using FluentAssertions;
    using global::KafkaFlow.Retry.Forever;
    using Xunit;

    public class RetryForeverDefinitionBuilderTests
    {
        [Fact]
        public void RetryForeverDefinitionBuilder_Build_ReturnsRetryForeverDefinition()
        {
            // Arrange
            var builder = new RetryForeverDefinitionBuilder();
            var exception = new Exception();
            var retryContext = new RetryContext(exception);

            builder.WithTimeBetweenTriesPlan(new TimeSpan[0])
            .WithTimeBetweenTriesPlan(new Func<int, TimeSpan>(_ => new TimeSpan()))
            .Handle<Exception>()
            .Handle(new Func<Exception, bool>(_ => 1 == 1))
            .Handle(new Func<RetryContext, bool>((d) => d == retryContext));

            // Act
            var result = builder.Build();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType(typeof(RetryForeverDefinition));
            result.ShouldRetry(retryContext).Should().BeTrue();
            result.TimeBetweenTriesPlan.Should().NotBeNull();
        }
    }
}