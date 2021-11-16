namespace KafkaFlow.Retry.UnitTests.KafkaFlow.Retry
{
    using System;
    using FluentAssertions;
    using Xunit;

    public class RetryContextTests
    {
        [Fact]
        public void RetryContext_Ctor_WithException_Success()
        {
            //Arrange
            var exception = new Exception();

            // Act
            var retryContext = new RetryContext(exception);

            // Assert
            retryContext.Should().NotBeNull();
            retryContext.Exception.Should().Be(exception);
        }

        [Fact]
        public void RetryContext_Ctor_WithNullException_ThrowException()
        {
            // Act
            Action act = () => new RetryContext(null);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }
    }
}