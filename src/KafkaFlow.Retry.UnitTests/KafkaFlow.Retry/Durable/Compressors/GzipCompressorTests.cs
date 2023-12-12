using System;
using FluentAssertions;
using global::KafkaFlow.Retry.Durable.Compression;
using Xunit;

namespace KafkaFlow.Retry.UnitTests.KafkaFlow.Retry.Durable.Compressors;

public class GzipCompressorTests
{
    private readonly IGzipCompressor gzipCompressor = new GzipCompressor();

    [Fact]
    public void GzipCompressor_Compress_Success()
    {
        // Arrange
        var data = new byte[0];

        // Act
        var result = this.gzipCompressor.Compress(data);

        // Assert
        result.Should().BeEquivalentTo(data);
    }

    [Fact]
    public void GzipCompressor_Compress_WithNullArg_ThrowsException()
    {
        // Act
        Action act = () => this.gzipCompressor.Compress(null);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GzipCompressor_Decompress_Success()
    {
        // Arrange
        var data = new byte[0];

        // Act
        var result = this.gzipCompressor.Decompress(data);

        // Assert
        result.Should().BeEquivalentTo(data);
    }

    [Fact]
    public void GzipCompressor_Decompress_WithNullArg_ThrowsException()
    {
        // Act
        Action act = () => this.gzipCompressor.Decompress(null);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}