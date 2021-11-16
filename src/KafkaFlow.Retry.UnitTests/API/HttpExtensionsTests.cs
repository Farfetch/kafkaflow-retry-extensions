namespace KafkaFlow.Retry.UnitTests.API
{
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using global::KafkaFlow.Retry.API;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Internal;
    using Xunit;

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
            HttpExtensions.AddQueryParams(context.Request, expectedName, expectedValue);

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
            var result = HttpExtensions.ExtendResourcePath(expectedResource, expectedExtension);

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
                new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
                {
                    { "newParamKey", new Microsoft.Extensions.Primitives.StringValues(expectedParamValue) }
                });

            // Act
            var result = HttpExtensions.ReadQueryParams(context.Request, expectedParamKey);

            // Assert
            result.Should().NotBeEmpty();
            result.FirstOrDefault().Should().Be(expectedParamValue);
        }
    }
}