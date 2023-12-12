using System;
using System.Threading.Tasks;
using FluentAssertions;
using global::KafkaFlow.Retry.Durable;
using global::KafkaFlow.Retry.Durable.Compression;
using Moq;
using Xunit;

namespace KafkaFlow.Retry.UnitTests.KafkaFlow.Retry.Durable;

public class RetryDurableConsumerCompressorMiddlewareTests
{
    [Fact]
    internal void RetryDurableConsumerCompressorMiddleware_Ctor_Tests()
    {
        // Act
        Action act = () => new RetryDurableConsumerCompressorMiddleware(null);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    internal async Task RetryDurableConsumerCompressorMiddleware_Invoke_Tests()
    {
        // Arrange
        var decompressed = new byte[] { 0x20 };

        var mockIGzipCompressor = new Mock<IGzipCompressor>();
        mockIGzipCompressor
            .Setup(x => x.Decompress(It.IsAny<byte[]>()))
            .Returns(decompressed);

        var mockIMessageContext = new Mock<IMessageContext>();

        var compressorMiddleware = new RetryDurableConsumerCompressorMiddleware(mockIGzipCompressor.Object);

        // Act
        await compressorMiddleware.Invoke(mockIMessageContext.Object, _ => Task.CompletedTask).ConfigureAwait(false);

        // Assert
        mockIMessageContext.Verify(c => c.SetMessage(null, decompressed), Times.Once);
    }
}