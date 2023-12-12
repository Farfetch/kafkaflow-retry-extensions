using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using global::KafkaFlow.Retry.UnitTests.API.Surrogate;
using global::KafkaFlow.Retry.UnitTests.API.Utilities;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace KafkaFlow.Retry.UnitTests.API;

public class RetryRequestHandlerBaseTests
{
    private const string HttpMethod = "GET";
    private const string ResourcePath = "/retry/resource";

    [Fact]
    public async Task RetryRequestHandlerBase_HandleAsync_CallsHandleRequestAsync()
    {
        // Arrange
        var dto = new DtoSurrogate
        {
            Text = Durable.Repository.Model.RetryQueueStatus.Active
        };

        var mockHttpContext = HttpContextHelper.MockHttpContext(ResourcePath, HttpMethod, requestBody: dto);

        var httpResponse = new Mock<HttpResponse>();
        string actualValue = null;

        httpResponse
            .Setup(_ => _.Body.WriteAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Callback((byte[] data, int _, int length, CancellationToken cancellation) =>
            {
                if (length > 0 && !cancellation.IsCancellationRequested)
                {
                    actualValue = Encoding.UTF8.GetString(data);
                }
            })
            .Returns(Task.CompletedTask);

        mockHttpContext
            .SetupGet(ctx => ctx.Response)
            .Returns(httpResponse.Object);

        var surrogate = new RetryRequestHandlerSurrogate(string.Empty, "resource");

        // Act
        var result = await surrogate.HandleAsync(mockHttpContext.Object.Request, mockHttpContext.Object.Response);

        // Assert
        result.Should().BeTrue();
        Assert.Equal(Newtonsoft.Json.JsonConvert.SerializeObject(dto), actualValue);
    }

    [Fact]
    public async Task RetryRequestHandlerBase_HandleAsync_WithWrongHttpMethod_DoesNotHandle()
    {
        // Arrange
        var wrongMethod = "someMethod";

        var httpContext = await HttpContextHelper.CreateContext(ResourcePath, wrongMethod);

        var surrogate = new RetryRequestHandlerSurrogate(string.Empty, "resource");

        // Act
        var result = await surrogate.HandleAsync(httpContext.Request, httpContext.Response).ConfigureAwait(false);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RetryRequestHandlerBase_HandleAsync_WithWrongResourcePath_DoesNotHandle()
    {
        // Arrange
        var wrongPath = "/somePath";

        var httpContext = await HttpContextHelper.CreateContext(wrongPath, HttpMethod);

        var surrogate = new RetryRequestHandlerSurrogate(string.Empty, "resource");

        // Act
        var result = await surrogate.HandleAsync(httpContext.Request, httpContext.Response).ConfigureAwait(false);

        // Assert
        result.Should().BeFalse();
    }
}