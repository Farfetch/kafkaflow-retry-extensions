namespace KafkaFlow.Retry.UnitTests.API.Handlers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using FluentAssertions;
    using global::KafkaFlow.Retry.API.Adapters.GetItems;
    using global::KafkaFlow.Retry.API.Dtos;
    using global::KafkaFlow.Retry.API.Dtos.Common;
    using global::KafkaFlow.Retry.API.Handlers;
    using global::KafkaFlow.Retry.Durable.Common;
    using global::KafkaFlow.Retry.Durable.Repository;
    using global::KafkaFlow.Retry.Durable.Repository.Actions.Read;
    using global::KafkaFlow.Retry.Durable.Repository.Model;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using Newtonsoft.Json;
    using Xunit;

    [ExcludeFromCodeCoverage]
    public class GetItemsHandlerTests
    {
        private readonly Mock<IGetItemsInputAdapter> getItemsInputAdapter = new Mock<IGetItemsInputAdapter>();
        private readonly Mock<IGetItemsRequestDtoReader> getItemsRequestDtoReader = new Mock<IGetItemsRequestDtoReader>();
        private readonly Mock<IGetItemsResponseDtoAdapter> getItemsResponseDtoReader = new Mock<IGetItemsResponseDtoAdapter>();
        private readonly string httpMethod = "GET";
        private readonly string resourcePath = "/retry/items";
        private readonly Mock<IRetryDurableQueueRepositoryProvider> retryDurableQueueRepositoryProvider = new Mock<IRetryDurableQueueRepositoryProvider>();

        public Mock<IGetItemsRequestDtoReader> GetItemsRequestDtoReader => getItemsRequestDtoReader;

        [Fact]
        public async Task GetItemsHandler_HandleAsync_Success()
        {
            // Arrange
            var httpContext = this.CreateHttpContext();

            var getItemsRequestDto = this.CreateRequestDto();
            var getQueuesInput = this.CreateInput();
            var getQueuesResult = this.CreateResult();
            var expectedGetItemsResponseDto = this.CreateResponseDto();

            GetItemsRequestDtoReader
                .Setup(mock => mock.Read(httpContext.Request))
                .Returns(getItemsRequestDto);

            getItemsInputAdapter
                .Setup(mock => mock.Adapt(getItemsRequestDto))
                .Returns(getQueuesInput);

            retryDurableQueueRepositoryProvider
                .Setup(mock => mock.GetQueuesAsync(getQueuesInput))
                .ReturnsAsync(getQueuesResult);

            getItemsResponseDtoReader
                .Setup(mock => mock.Adapt(getQueuesResult))
                .Returns(expectedGetItemsResponseDto);

            var handler = new GetItemsHandler(
                retryDurableQueueRepositoryProvider.Object,
                GetItemsRequestDtoReader.Object,
                getItemsInputAdapter.Object,
                getItemsResponseDtoReader.Object
                );

            // Act
            var handled = await handler.HandleAsync(httpContext.Request, httpContext.Response).ConfigureAwait(false);

            // Assert
            handled.Should().BeTrue();
            GetItemsRequestDtoReader.Verify(mock => mock.Read(httpContext.Request), Times.Once());
            getItemsInputAdapter.Verify(mock => mock.Adapt(getItemsRequestDto), Times.Once());
            retryDurableQueueRepositoryProvider.Verify(mock => mock.GetQueuesAsync(getQueuesInput), Times.Once());
            getItemsResponseDtoReader.Verify(mock => mock.Adapt(getQueuesResult), Times.Once());

            await this.AssertResponse(httpContext.Response, expectedGetItemsResponseDto).ConfigureAwait(false);
        }

        [Theory]
        [ClassData(typeof(DependenciesThrowingExceptionsData))]
        public async Task GetItemsHandler_HandleAsync_WithException_ReturnsExpectedStatusCode(
            IGetItemsRequestDtoReader getItemsRequestDtoReader,
            IGetItemsInputAdapter getItemsInputAdapter,
            IRetryDurableQueueRepositoryProvider retryQueueDataProvider,
            IGetItemsResponseDtoAdapter getItemsResponseDtoAdapter,
            int expectedStatusCode)
        {
            // Arrange
            var httpContext = this.CreateHttpContext();

            var handler = new GetItemsHandler(
                retryQueueDataProvider,
                getItemsRequestDtoReader,
                getItemsInputAdapter,
                getItemsResponseDtoAdapter
                );

            // Act
            await handler.HandleAsync(httpContext.Request, httpContext.Response);

            // Assert
            httpContext.Response.StatusCode.Should().Be(expectedStatusCode);
        }

        private async Task AssertResponse(HttpResponse response, GetItemsResponseDto expectedResponseDto)
        {
            //Rewind the stream
            response.Body.Seek(0, SeekOrigin.Begin);

            GetItemsResponseDto responseDto;

            using (var reader = new StreamReader(response.Body, Encoding.UTF8))
            {
                var requestMessage = await reader.ReadToEndAsync().ConfigureAwait(false);

                responseDto = JsonConvert.DeserializeObject<GetItemsResponseDto>(requestMessage);
            }

            responseDto.Should().BeEquivalentTo(expectedResponseDto);
        }

        private HttpContext CreateHttpContext()
        {
            var context = new DefaultHttpContext();

            context.Request.Path = this.resourcePath;
            context.Request.Method = this.httpMethod;

            context.Response.Body = new MemoryStream();

            return context;
        }

        private GetQueuesInput CreateInput()
        {
            return new GetQueuesInput(
                RetryQueueStatus.Active,
                new RetryQueueItemStatus[] { RetryQueueItemStatus.Waiting },
                GetQueuesSortOption.ByCreationDate_Descending,
                100)
            {
                TopItemsByQueue = 1000
            };
        }

        private GetItemsRequestDto CreateRequestDto()
        {
            return new GetItemsRequestDto
            {
                ItemsStatuses = new RetryQueueItemStatus[] { RetryQueueItemStatus.Waiting },
                SeverityLevels = new SeverityLevel[] { SeverityLevel.High },
                TopItemsByQueue = 100,
                TopQueues = 1000
            };
        }

        private GetItemsResponseDto CreateResponseDto()
        {
            var queueItemsDto = new[]
            {
                new RetryQueueItemDto(),
                new RetryQueueItemDto()
            };

            return new GetItemsResponseDto(queueItemsDto);
        }

        private GetQueuesResult CreateResult()
        {
            return new GetQueuesResult(this.CreateRetryQueues());
        }

        private IEnumerable<RetryQueue> CreateRetryQueues()
        {
            var retryQueueItems = new[]
            {
                new RetryQueueItem(Guid.NewGuid(), 3, DateTime.UtcNow, 1, DateTime.UtcNow, DateTime.UtcNow, RetryQueueItemStatus.Waiting, SeverityLevel.High, "description"),
                new RetryQueueItem(Guid.NewGuid(), 0, DateTime.UtcNow, 2, null, DateTime.UtcNow, RetryQueueItemStatus.Waiting, SeverityLevel.High, "description"),
            };

            return new[]
            {
                new RetryQueue(Guid.NewGuid(), "orderGroupKey", "searchGroupKey", DateTime.UtcNow, DateTime.UtcNow, RetryQueueStatus.Active, retryQueueItems)
            };
        }

        private class DependenciesThrowingExceptionsData : IEnumerable<object[]>
        {
            private readonly Mock<IRetryDurableQueueRepositoryProvider> dataProvider;
            private readonly Mock<IRetryDurableQueueRepositoryProvider> dataProviderWithException;
            private readonly Mock<IGetItemsInputAdapter> inputAdapter;
            private readonly Mock<IGetItemsInputAdapter> inputAdapterWithException;
            private readonly Mock<IGetItemsRequestDtoReader> requestDtoReader;
            private readonly Mock<IGetItemsRequestDtoReader> requestDtoReaderWithException;
            private readonly Mock<IGetItemsResponseDtoAdapter> responseDtoAdapter;
            private readonly Mock<IGetItemsResponseDtoAdapter> responseDtoAdapterWithException;

            public DependenciesThrowingExceptionsData()
            {
                this.requestDtoReader = new Mock<IGetItemsRequestDtoReader>();
                this.inputAdapter = new Mock<IGetItemsInputAdapter>();
                this.dataProvider = new Mock<IRetryDurableQueueRepositoryProvider>();
                this.responseDtoAdapter = new Mock<IGetItemsResponseDtoAdapter>();

                this.requestDtoReaderWithException = new Mock<IGetItemsRequestDtoReader>();
                requestDtoReaderWithException
                    .Setup(mock => mock.Read(It.IsAny<HttpRequest>()))
                    .Throws(new Exception());

                this.inputAdapterWithException = new Mock<IGetItemsInputAdapter>();
                inputAdapterWithException
                    .Setup(mock => mock.Adapt(It.IsAny<GetItemsRequestDto>()))
                    .Throws(new Exception());

                this.dataProviderWithException = new Mock<IRetryDurableQueueRepositoryProvider>();
                dataProviderWithException
                    .Setup(mock => mock.GetQueuesAsync(It.IsAny<GetQueuesInput>()))
                    .ThrowsAsync(new Exception());

                this.responseDtoAdapterWithException = new Mock<IGetItemsResponseDtoAdapter>();
                responseDtoAdapterWithException
                    .Setup(mock => mock.Adapt(It.IsAny<GetQueuesResult>()))
                    .Throws(new Exception());
            }

            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] // success case
                {
                    requestDtoReader.Object,
                    inputAdapter.Object,
                    dataProvider.Object,
                    responseDtoAdapter.Object,
                    (int)HttpStatusCode.OK
                };
                yield return new object[]
                {
                    requestDtoReaderWithException.Object,
                    inputAdapter.Object,
                    dataProvider.Object,
                    responseDtoAdapter.Object,
                    (int)HttpStatusCode.InternalServerError
                };
                yield return new object[]
                {
                    requestDtoReader.Object,
                    inputAdapterWithException.Object,
                    dataProvider.Object,
                    responseDtoAdapter.Object,
                    (int)HttpStatusCode.InternalServerError
                };
                yield return new object[]
                {
                    requestDtoReader.Object,
                    inputAdapter.Object,
                    dataProviderWithException.Object,
                    responseDtoAdapter.Object,
                    (int)HttpStatusCode.InternalServerError
                };
                yield return new object[]
                {
                    requestDtoReader.Object,
                    inputAdapter.Object,
                    dataProvider.Object,
                    responseDtoAdapterWithException.Object,
                    (int)HttpStatusCode.InternalServerError
                };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}