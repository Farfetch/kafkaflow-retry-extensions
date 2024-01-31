using System;
using KafkaFlow.Retry.Forever;

namespace KafkaFlow.Retry.UnitTests.KafkaFlow.Retry.Forever;

public class RetryForeverDefinitionBuilderTests
{
    [Fact]
    public void RetryForeverDefinitionBuilder_Build_ReturnsRetryForeverDefinition()
    {
        // Arrange
        var builder = new RetryForeverDefinitionBuilder();
        var exception = new Exception();
        var retryContext = new RetryContext(exception);

        builder.WithTimeBetweenTriesPlan()
            .WithTimeBetweenTriesPlan(_ => new TimeSpan())
            .Handle<Exception>()
            .Handle(new Func<Exception, bool>(_ => true))
            .Handle(d => d == retryContext);

        // Act
        var result = builder.Build();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType(typeof(RetryForeverDefinition));
        result.ShouldRetry(retryContext).Should().BeTrue();
        result.TimeBetweenTriesPlan.Should().NotBeNull();
    }
}