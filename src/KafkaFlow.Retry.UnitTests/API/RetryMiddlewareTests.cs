namespace KafkaFlow.Retry.UnitTests.API
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using global::KafkaFlow.Retry.API;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using Xunit;

    [ExcludeFromCodeCoverage]
    public class RetryMiddlewareTests
    {
        [Fact]
        public async Task RetryMiddleware_InvokeAsync_CallsHttpRequestHandler()
        {
            // Arrange
            var mockHttpRequestHandler = new Mock<IHttpRequestHandler>();

            var context = new DefaultHttpContext();

            RequestDelegate next = (HttpContext _) => Task.CompletedTask;

            var middleware = new RetryMiddleware(next, mockHttpRequestHandler.Object);

            // Act
            await middleware.InvokeAsync(context).ConfigureAwait(false);

            // Assert
            mockHttpRequestHandler.Verify(mock => mock.HandleAsync(context.Request, context.Response), Times.Once());
        }
    }
}