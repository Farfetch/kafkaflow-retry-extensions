﻿using System;
using System.Collections.Generic;
using KafkaFlow.Retry.API.Adapters.Common.Parsers;

namespace KafkaFlow.Retry.UnitTests.API.Adapters.Common.Parses;

public class EnumParserTests
{
    private readonly EnumTests[] _defaultEnum = { EnumTests.Value1 };

    private readonly EnumParser<EnumTests> _enumParser = new();

    [Fact]
    public void EnumParser_Parse_Success()
    {
        // Arrange
        var queryParams = new[] { "1", "2", "3" };
        var expectedItems = new[] { EnumTests.Value1, EnumTests.Value2, EnumTests.Value3 };

        // Act
        var result = _enumParser.Parse(queryParams, _defaultEnum);

        // Assert
        result.Should().BeEquivalentTo(expectedItems);
    }

    [Fact]
    public void EnumParser_Parse_WithEmptyItems_ReturnsDefaultValue()
    {
        // Arrange
        var queryParams = new string[0];

        // Act
        var result = _enumParser.Parse(queryParams, _defaultEnum);

        // Assert
        result.Should().BeEquivalentTo(_defaultEnum);
    }

    [Theory]
    [InlineData(typeof(IEnumerable<string>))]
    [InlineData(typeof(IEnumerable<EnumTests>))]
    public void EnumParser_Parse_WithNullArgs_ThrowsException(Type nullType)
    {
        // Act
        Action act = () => _enumParser.Parse(
            nullType.Equals(typeof(IEnumerable<string>)) ? null : new string[0],
            nullType.Equals(typeof(IEnumerable<EnumTests>)) ? null : new EnumTests[0]);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    private enum EnumTests
    {
        Value1 = 1,
        Value2 = 2,
        Value3 = 3
    }
}