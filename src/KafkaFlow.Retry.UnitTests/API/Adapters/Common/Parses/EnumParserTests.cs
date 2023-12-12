using System;
using System.Collections.Generic;
using FluentAssertions;
using KafkaFlow.Retry.API.Adapters.Common.Parsers;
using Xunit;

namespace KafkaFlow.Retry.UnitTests.API.Adapters.Common.Parses;

public class EnumParserTests
{
    private readonly EnumTests[] defaultEnum = new[] { EnumTests.Value1 };

    private readonly EnumParser<EnumTests> enumParser = new EnumParser<EnumTests>();

    private enum EnumTests
    {
        Value1 = 1,
        Value2 = 2,
        Value3 = 3
    }

    [Fact]
    public void EnumParser_Parse_Success()
    {
            // Arrange
            var queryParams = new[] { "1", "2", "3" };
            var expectedItems = new[] { EnumTests.Value1, EnumTests.Value2, EnumTests.Value3 };

            // Act
            var result = this.enumParser.Parse(queryParams, this.defaultEnum);

            // Assert
            result.Should().BeEquivalentTo(expectedItems);
        }

    [Fact]
    public void EnumParser_Parse_WithEmptyItems_ReturnsDefaultValue()
    {
            // Arrange
            var queryParams = new string[0];

            // Act
            var result = this.enumParser.Parse(queryParams, this.defaultEnum);

            // Assert
            result.Should().BeEquivalentTo(this.defaultEnum);
        }

    [Theory]
    [InlineData(typeof(IEnumerable<string>))]
    [InlineData(typeof(IEnumerable<EnumTests>))]
    public void EnumParser_Parse_WithNullArgs_ThrowsException(Type nullType)
    {
            // Act
            Action act = () => this.enumParser.Parse(
                   nullType.Equals(typeof(IEnumerable<string>)) ? null : new String[0],
                   nullType.Equals(typeof(IEnumerable<EnumTests>)) ? null : new EnumTests[0]);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }
}