﻿using System.Text;
using KafkaFlow.Retry.Durable.Encoders;

namespace KafkaFlow.Retry.UnitTests.KafkaFlow.Retry.Durable.Encoders;

public class Utf8EncoderTests
{
    private readonly Utf8Encoder _utf8Encoder = new();

    [Fact]
    public void Utf8Encoder_Deconde_Success()
    {
        // Arrange
        var data = new byte[0];

        // Act
        var result = _utf8Encoder.Decode(data);

        // Assert
        result.Should().BeEquivalentTo(Encoding.UTF8.GetString(data));
    }

    [Fact]
    public void Utf8Encoder_Enconde_Success()
    {
        // Arrange
        var data = "new";

        // Act
        var result = _utf8Encoder.Encode(data);

        // Assert
        result.Should().BeEquivalentTo(Encoding.UTF8.GetBytes(data));
    }
}