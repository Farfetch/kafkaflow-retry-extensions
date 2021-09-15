namespace KafkaFlow.Retry.UnitTests.API
{
    using System.Threading.Tasks;
    using FluentAssertions;
    using global::KafkaFlow.Retry.UnitTests.API.Surrogate;
    using global::KafkaFlow.Retry.UnitTests.API.Utilities;
    using Xunit;

    public class RetryRequestHandlerBaseTests
    {
        private const string HttpMethod = "GET";
        private const string ResourcePath = "/retry";

        [Fact]
        public async Task RetryRequestHandlerBase_HandleAsync_CallsHandleRequestAsync()
        {
            // Arrange
            var dto = new DtoSurrogate
            {
                Text = Durable.Repository.Model.RetryQueueStatus.Active
            };

            var httpContext = await HttpContextHelper.CreateContext(ResourcePath, HttpMethod, dto).ConfigureAwait(false);

            var surrogate = new RetryRequestHandlerSurrogate();

            // Act
            var result = await surrogate.HandleAsync(httpContext.Request, httpContext.Response);

            // Assert
            result.Should().BeTrue();

            //var actualValue = await HttpContextHelper.ReadResponse<DtoSurrogate>(httpContext.Response);
            //Assert.Equal(dto, actualValue);
        }

        [Fact]
        public async Task RetryRequestHandlerBase_HandleAsync_WithWrongHttpMethod_DoesNotHandle()
        {
            // Arrange
            var wrongMethod = "someMethod";

            var httpContext = await HttpContextHelper.CreateContext(ResourcePath, wrongMethod);

            var surrogate = new RetryRequestHandlerSurrogate();

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

            var surrogate = new RetryRequestHandlerSurrogate();

            // Act
            var result = await surrogate.HandleAsync(httpContext.Request, httpContext.Response).ConfigureAwait(false);

            // Assert
            result.Should().BeFalse();
        }
    }
}