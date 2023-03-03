namespace KafkaFlow.Retry.UnitTests.KafkaFlow.Retry.Forever
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using global::KafkaFlow.Retry.Forever;
    using Xunit;

    public class RetryForeverDefinitionTests
    {
        public static IEnumerable<object[]> DataTest() => new List<object[]>
        {
            new object[]
            {
                null,
                new List<Func<RetryContext, bool>>()
            },
            new object[]
            {
                new Func<int, TimeSpan>((_) => new TimeSpan()),
                null,
            }
        };

        [Theory]
        [MemberData(nameof(DataTest))]
        public void RetryForeverDefinition_Ctor_Tests(
            Func<int, TimeSpan> timeBetweenTriesPlan,
            IReadOnlyCollection<Func<RetryContext, bool>> retryWhenExceptions)
        {
            // Act
            Action act = () => new RetryForeverDefinition(timeBetweenTriesPlan, retryWhenExceptions);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void RetryForeverDefinition_ShouldRetry_ReturnFalse()
        {
            // Arrange
            var exception = new Exception();
            var retryContext = new RetryContext(exception);
            var timeBetweenTriesPlan = new Func<int, TimeSpan>((_) => new TimeSpan());
            var retryWhenExceptions = new List<Func<RetryContext, bool>>();

            var retry = new RetryForeverDefinition(timeBetweenTriesPlan, retryWhenExceptions);

            // Act
            var result = retry.ShouldRetry(retryContext);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void RetryForeverDefinition_ShouldRetry_ReturnTrue()
        {
            // Arrange
            var exception = new Exception();
            var retryContext = new RetryContext(exception);
            var timeBetweenTriesPlan = new Func<int, TimeSpan>((_) => new TimeSpan());
            var retryWhenExceptions = new List<Func<RetryContext, bool>>
            {
                new Func<RetryContext, bool>((d) => d == retryContext )
            };

            var retry = new RetryForeverDefinition(timeBetweenTriesPlan, retryWhenExceptions);

            // Act
            var result = retry.ShouldRetry(retryContext);

            // Assert
            result.Should().BeTrue();
        }
    }
}