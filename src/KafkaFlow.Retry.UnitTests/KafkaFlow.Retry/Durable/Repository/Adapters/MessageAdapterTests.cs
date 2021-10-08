namespace KafkaFlow.Retry.UnitTests.KafkaFlow.Retry.Durable.Repository.Adapters
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using FluentAssertions;
    using global::KafkaFlow.Retry.Durable.Compression;
    using global::KafkaFlow.Retry.Durable.Repository.Adapters;
    using global::KafkaFlow.Retry.Durable.Serializers;
    using Moq;
    using Xunit;

    [ExcludeFromCodeCoverage]
    public class MessageAdapterTests
    {
        public readonly static IEnumerable<object[]> DataTest = new List<object[]>
        {
            new object[]
            {
                null,
                Mock.Of<IProtobufNetSerializer>()
            },
            new object[]
            {
                Mock.Of<IGzipCompressor>(),
                null
            }
        };

        private readonly Mock<IGzipCompressor> gzipCompressor = new Mock<IGzipCompressor>();
        private readonly Mock<IProtobufNetSerializer> protobufNetSerializer = new Mock<IProtobufNetSerializer>();

        [Fact]
        public void MessageAdapter_AdaptMessageFromRepository_Success()
        {
            // Arrange
            var expectedDecompress = new byte[2];

            gzipCompressor.Setup(d => d.Decompress(It.IsAny<byte[]>())).Returns(expectedDecompress);
            var adapter = new MessageAdapter(gzipCompressor.Object,
                protobufNetSerializer.Object);

            // Act
            var decompress = adapter.AdaptMessageFromRepository(new byte[1]);

            // Assert
            decompress.Should().BeEquivalentTo(expectedDecompress);
            gzipCompressor.Verify(d => d.Decompress(It.IsAny<byte[]>()), Times.Once);
        }

        [Fact]
        public void MessageAdapter_AdaptMessageToRepository_Success()
        {
            // Arrange
            var expectedCompress = new byte[2];

            gzipCompressor.Setup(d => d.Compress(It.IsAny<byte[]>())).Returns(expectedCompress);
            protobufNetSerializer.Setup(d => d.Serialize(It.IsAny<byte[]>())).Returns(expectedCompress);

            var adapter = new MessageAdapter(gzipCompressor.Object,
                protobufNetSerializer.Object);

            // Act
            var decompress = adapter.AdaptMessageToRepository(new byte[1]);

            // Assert
            decompress.Should().BeEquivalentTo(expectedCompress);
            gzipCompressor.Verify(d => d.Compress(It.IsAny<byte[]>()), Times.Once);
            protobufNetSerializer.Verify(d => d.Serialize(It.IsAny<byte[]>()), Times.Once);
        }

        [Theory]
        [MemberData(nameof(DataTest))]
        public void MessageAdapter_Ctor_WithArgumentNull_ThrowsException(
            object gzipCompressor,
            object protobufNetSerializer)
        {
            // Arrange & Act
            Action act = () => new MessageAdapter((IGzipCompressor)gzipCompressor,
                (IProtobufNetSerializer)protobufNetSerializer);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }
    }
}