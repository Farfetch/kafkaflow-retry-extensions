using System;
using KafkaFlow.Retry.Durable.Compression;

namespace KafkaFlow.Retry.UnitTests.KafkaFlow.Retry.Durable.Compressors;

public class GzipCompressorTests
{
    private readonly IGzipCompressor _gzipCompressor = new GzipCompressor();

    [Fact]
    public void GzipCompressor_Compress_Success()
    {
        // Arrange
        var data = new byte[0];

        // Act
        var result = _gzipCompressor.Compress(data);

        // Assert
        result.Should().BeEquivalentTo(data);
    }

    [Fact]
    public void GzipCompressor_Compress_WithNullArg_ThrowsException()
    {
        // Act
        Action act = () => _gzipCompressor.Compress(null);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GzipCompressor_Decompress_Success()
    {
        // Arrange
        var data = new byte[0];

        // Act
        var result = _gzipCompressor.Decompress(data);

        // Assert
        result.Should().BeEquivalentTo(data);
    }

    [Fact]
    public void GzipCompressor_Decompress_WithNullArg_ThrowsException()
    {
        // Act
        Action act = () => _gzipCompressor.Decompress(null);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}