using System.Collections.Generic;
using System.Linq;
using KafkaFlow.Retry.API;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Primitives;

namespace KafkaFlow.Retry.UnitTests.API;

public class HttpExtensionsTests
{
    [Fact]
    public void HttpExtensions_AddQueryParams_Success()
    {
        // Arrange
        var expectedName = "Id";
        var expectedValue = "1";
        var context = new DefaultHttpContext();

        // Act
        context.Request.AddQueryParams(expectedName, expectedValue);

        // Assert
        context.Request.QueryString.Should().NotBeNull();
        context.Request.QueryString.Value.Contains($"{expectedName}={expectedValue}");
    }

    [Fact]
    public void HttpExtensions_ExtendResourcePath_Success()
    {
        // Arrange
        var expectedResource = "newResource";
        var expectedExtension = "newExtension";

        // Act
        var result = expectedResource.ExtendResourcePath(expectedExtension);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be($"{expectedResource}/{expectedExtension}");
    }

    [Fact]
    public void HttpExtensions_ReadQueryParams_Success()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var expectedParamKey = "newParamKey";
        var expectedParamValue = "1";
        context.Request.Query = new QueryCollection(
            new Dictionary<string, StringValues>
            {
                { "newParamKey", new StringValues(expectedParamValue) }
            });

        // Act
        var result = context.Request.ReadQueryParams(expectedParamKey);

        // Assert
        result.Should().NotBeEmpty();
        result.FirstOrDefault().Should().Be(expectedParamValue);
    }
}