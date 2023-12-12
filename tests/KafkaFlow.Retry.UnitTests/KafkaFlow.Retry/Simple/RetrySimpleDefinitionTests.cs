using System;
using System.Collections.Generic;
using KafkaFlow.Retry.Simple;

namespace KafkaFlow.Retry.UnitTests.KafkaFlow.Retry.Simple;

public class RetrySimpleDefinitionTests
{
    public static IEnumerable<object[]> DataTest()
    {
        return new List<object[]>
        {
            new object[]
            {
                null,
                new Func<int, TimeSpan>(_ => new TimeSpan())
            },
            new object[]
            {
                new List<Func<RetryContext, bool>>(),
                null
            }
        };
    }

    [Theory]
    [MemberData(nameof(DataTest))]
    public void RetrySimpleDefinition_Ctor_Tests(
        IReadOnlyCollection<Func<RetryContext, bool>> retryWhenExceptions,
        Func<int, TimeSpan> timeBetweenTriesPlan)
    {
        // Act
        Action act = () => new RetrySimpleDefinition(1, retryWhenExceptions, true, timeBetweenTriesPlan);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RetrySimpleDefinition_Ctor_WithNegativeNumberOfRetries_ThrowException()
    {
        // Act
        Action act = () => new RetrySimpleDefinition(
            -1,
            new List<Func<RetryContext, bool>>(),
            true,
            _ => new TimeSpan());

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void RetrySimpleDefinition_ShouldRetry_ReturnFalse()
    {
        // Arrange
        var numberOfRetries = 1;
        var exception = new Exception();
        var retryContext = new RetryContext(exception);
        var timeBetweenTriesPlan = new Func<int, TimeSpan>(_ => new TimeSpan());
        var retryWhenExceptions = new List<Func<RetryContext, bool>>();

        var retry = new RetrySimpleDefinition(numberOfRetries, retryWhenExceptions, false, timeBetweenTriesPlan);

        // Act
        var result = retry.ShouldRetry(retryContext);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void RetrySimpleDefinition_ShouldRetry_ReturnTrue()
    {
        // Arrange
        var numberOfRetries = 1;
        var exception = new Exception();
        var retryContext = new RetryContext(exception);
        var timeBetweenTriesPlan = new Func<int, TimeSpan>(_ => new TimeSpan());
        var retryWhenExceptions = new List<Func<RetryContext, bool>>
        {
            d => d == retryContext
        };

        var retry = new RetrySimpleDefinition(numberOfRetries, retryWhenExceptions, false, timeBetweenTriesPlan);

        // Act
        var result = retry.ShouldRetry(retryContext);

        // Assert
        result.Should().BeTrue();
    }
}