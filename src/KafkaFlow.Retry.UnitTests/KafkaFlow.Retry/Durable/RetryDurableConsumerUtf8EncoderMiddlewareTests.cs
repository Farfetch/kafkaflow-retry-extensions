using System;
using System.Threading.Tasks;
using FluentAssertions;
using global::KafkaFlow.Retry.Durable;
using global::KafkaFlow.Retry.Durable.Encoders;
using Moq;
using Xunit;

namespace KafkaFlow.Retry.UnitTests.KafkaFlow.Retry.Durable;

public class RetryDurableConsumerUtf8EncoderMiddlewareTests
{
    [Fact]
    internal void RetryDurableConsumerUtf8EncoderMiddleware_Ctor_Tests()
    {
            // Act
            Action act = () => new RetryDurableConsumerUtf8EncoderMiddleware(null);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

    [Fact]
    internal async Task RetryDurableConsumerUtf8EncoderMiddleware_Invoke_Tests()
    {
            // Arrange
            var decoded = "encoded";

            var mockIUtf8Encoder = new Mock<IUtf8Encoder>();
            mockIUtf8Encoder
                .Setup(x => x.Decode(It.IsAny<byte[]>()))
                .Returns(decoded);

            var mockIMessageContext = new Mock<IMessageContext>();

            var utf8EncoderMiddleware = new RetryDurableConsumerUtf8EncoderMiddleware(mockIUtf8Encoder.Object);

            // Act
            await utf8EncoderMiddleware.Invoke(mockIMessageContext.Object, _ => Task.CompletedTask).ConfigureAwait(false);

            // Assert
            mockIMessageContext.Verify(c => c.SetMessage(null, decoded), Times.Once);
        }
}