using System.Threading.Tasks;
using KafkaFlow.Retry.API;
using Microsoft.AspNetCore.Http;
using Moq;

namespace KafkaFlow.Retry.UnitTests.API;

public class RetryMiddlewareTests
{
    [Fact]
    public async Task RetryMiddleware_InvokeAsync_CallsHttpRequestHandler()
    {
        // Arrange
        var mockHttpRequestHandler = new Mock<IHttpRequestHandler>();

        var context = new DefaultHttpContext();

        RequestDelegate next = _ => Task.CompletedTask;

        var middleware = new RetryMiddleware(next, mockHttpRequestHandler.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        mockHttpRequestHandler.Verify(mock => mock.HandleAsync(context.Request, context.Response), Times.Once());
    }
}