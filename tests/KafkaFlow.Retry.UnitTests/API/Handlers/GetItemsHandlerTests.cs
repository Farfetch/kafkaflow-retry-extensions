using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using KafkaFlow.Retry.API.Adapters.GetItems;
using KafkaFlow.Retry.API.Dtos;
using KafkaFlow.Retry.API.Dtos.Common;
using KafkaFlow.Retry.API.Handlers;
using KafkaFlow.Retry.Durable.Common;
using KafkaFlow.Retry.Durable.Repository;
using KafkaFlow.Retry.Durable.Repository.Actions.Read;
using KafkaFlow.Retry.Durable.Repository.Model;
using Microsoft.AspNetCore.Http;
using Moq;
using Newtonsoft.Json;

namespace KafkaFlow.Retry.UnitTests.API.Handlers;

public class GetItemsHandlerTests
{
    private readonly string _httpMethod = "GET";
    private readonly Mock<IGetItemsInputAdapter> _mockGetItemsInputAdapter = new();
    private readonly Mock<IGetItemsRequestDtoReader> _mockGetItemsRequestDtoReader = new();
    private readonly Mock<IGetItemsResponseDtoAdapter> _mockGetItemsResponseDtoReader = new();
    private readonly string _resourcePath = "/testendpoint/retry/items";
    private readonly Mock<IRetryDurableQueueRepositoryProvider> _retryDurableQueueRepositoryProvider = new();

    [Fact]
    public async Task GetItemsHandler_HandleAsync_WithEndpointPrefix_Success()
    {
        // Arrange
        var httpContext = CreateHttpContext();

        var getItemsRequestDto = CreateRequestDto();
        var getQueuesInput = CreateInput();
        var getQueuesResult = CreateResult();
        var expectedGetItemsResponseDto = CreateResponseDto();

        _mockGetItemsRequestDtoReader
            .Setup(mock => mock.Read(httpContext.Request))
            .Returns(getItemsRequestDto);

        _mockGetItemsInputAdapter
            .Setup(mock => mock.Adapt(getItemsRequestDto))
            .Returns(getQueuesInput);

        _retryDurableQueueRepositoryProvider
            .Setup(mock => mock.GetQueuesAsync(getQueuesInput))
            .ReturnsAsync(getQueuesResult);

        _mockGetItemsResponseDtoReader
            .Setup(mock => mock.Adapt(getQueuesResult))
            .Returns(expectedGetItemsResponseDto);

        var handler = new GetItemsHandler(
            _retryDurableQueueRepositoryProvider.Object,
            _mockGetItemsRequestDtoReader.Object,
            _mockGetItemsInputAdapter.Object,
            _mockGetItemsResponseDtoReader.Object,
            "testendpoint"
        );

        // Act
        var handled = await handler.HandleAsync(httpContext.Request, httpContext.Response);

        // Assert
        handled.Should().BeTrue();
        _mockGetItemsRequestDtoReader.Verify(mock => mock.Read(httpContext.Request), Times.Once());
        _mockGetItemsInputAdapter.Verify(mock => mock.Adapt(getItemsRequestDto), Times.Once());
        _retryDurableQueueRepositoryProvider.Verify(mock => mock.GetQueuesAsync(getQueuesInput), Times.Once());
        _mockGetItemsResponseDtoReader.Verify(mock => mock.Adapt(getQueuesResult), Times.Once());

        await AssertResponseAsync(httpContext.Response, expectedGetItemsResponseDto);
    }

    [Theory]
    [ClassData(typeof(DependenciesThrowingExceptionsData))]
    public async Task GetItemsHandler_HandleAsync_WithExceptionAndEndpointPrefix_ReturnsExpectedStatusCode(
        IGetItemsRequestDtoReader getItemsRequestDtoReader,
        IGetItemsInputAdapter getItemsInputAdapter,
        IRetryDurableQueueRepositoryProvider retryQueueDataProvider,
        IGetItemsResponseDtoAdapter getItemsResponseDtoAdapter,
        int expectedStatusCode)
    {
        // Arrange
        var httpContext = CreateHttpContext();

        var handler = new GetItemsHandler(
            retryQueueDataProvider,
            getItemsRequestDtoReader,
            getItemsInputAdapter,
            getItemsResponseDtoAdapter,
            "testendpoint"
        );

        // Act
        await handler.HandleAsync(httpContext.Request, httpContext.Response);

        // Assert
        httpContext.Response.StatusCode.Should().Be(expectedStatusCode);
    }

    private async Task AssertResponseAsync(HttpResponse response, GetItemsResponseDto expectedResponseDto)
    {
        //Rewind the stream
        response.Body.Seek(0, SeekOrigin.Begin);

        GetItemsResponseDto responseDto;

        using (var reader = new StreamReader(response.Body, Encoding.UTF8))
        {
            var requestMessage = await reader.ReadToEndAsync();

            responseDto = JsonConvert.DeserializeObject<GetItemsResponseDto>(requestMessage);
        }

        responseDto.Should().BeEquivalentTo(expectedResponseDto);
    }

    private HttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();

        context.Request.Path = _resourcePath;
        context.Request.Method = _httpMethod;

        context.Response.Body = new MemoryStream();

        return context;
    }

    private GetQueuesInput CreateInput()
    {
        return new GetQueuesInput(
            RetryQueueStatus.Active,
            new[] { RetryQueueItemStatus.Waiting },
            GetQueuesSortOption.ByCreationDateDescending,
            100)
        {
            TopItemsByQueue = 1000
        };
    }

    private GetItemsRequestDto CreateRequestDto()
    {
        return new GetItemsRequestDto
        {
            ItemsStatuses = new[] { RetryQueueItemStatus.Waiting },
            SeverityLevels = new[] { SeverityLevel.High },
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
        return new GetQueuesResult(CreateRetryQueues());
    }

    private IEnumerable<RetryQueue> CreateRetryQueues()
    {
        var retryQueueItems = new[]
        {
            new RetryQueueItem(Guid.NewGuid(), 3, DateTime.UtcNow, 1, DateTime.UtcNow, DateTime.UtcNow,
                RetryQueueItemStatus.Waiting, SeverityLevel.High, "description"),
            new RetryQueueItem(Guid.NewGuid(), 0, DateTime.UtcNow, 2, null, DateTime.UtcNow,
                RetryQueueItemStatus.Waiting, SeverityLevel.High, "description")
        };

        return new[]
        {
            new RetryQueue(Guid.NewGuid(), "orderGroupKey", "searchGroupKey", DateTime.UtcNow, DateTime.UtcNow,
                RetryQueueStatus.Active, retryQueueItems)
        };
    }

    private class DependenciesThrowingExceptionsData : IEnumerable<object[]>
    {
        private readonly Mock<IRetryDurableQueueRepositoryProvider> _dataProvider;
        private readonly Mock<IRetryDurableQueueRepositoryProvider> _dataProviderWithException;
        private readonly Mock<IGetItemsInputAdapter> _inputAdapter;
        private readonly Mock<IGetItemsInputAdapter> _inputAdapterWithException;
        private readonly Mock<IGetItemsRequestDtoReader> _requestDtoReader;
        private readonly Mock<IGetItemsRequestDtoReader> _requestDtoReaderWithException;
        private readonly Mock<IGetItemsResponseDtoAdapter> _responseDtoAdapter;
        private readonly Mock<IGetItemsResponseDtoAdapter> _responseDtoAdapterWithException;

        public DependenciesThrowingExceptionsData()
        {
            _requestDtoReader = new Mock<IGetItemsRequestDtoReader>();
            _inputAdapter = new Mock<IGetItemsInputAdapter>();
            _dataProvider = new Mock<IRetryDurableQueueRepositoryProvider>();
            _responseDtoAdapter = new Mock<IGetItemsResponseDtoAdapter>();

            _requestDtoReaderWithException = new Mock<IGetItemsRequestDtoReader>();
            _requestDtoReaderWithException
                .Setup(mock => mock.Read(It.IsAny<HttpRequest>()))
                .Throws(new Exception());

            _inputAdapterWithException = new Mock<IGetItemsInputAdapter>();
            _inputAdapterWithException
                .Setup(mock => mock.Adapt(It.IsAny<GetItemsRequestDto>()))
                .Throws(new Exception());

            _dataProviderWithException = new Mock<IRetryDurableQueueRepositoryProvider>();
            _dataProviderWithException
                .Setup(mock => mock.GetQueuesAsync(It.IsAny<GetQueuesInput>()))
                .ThrowsAsync(new Exception());

            _responseDtoAdapterWithException = new Mock<IGetItemsResponseDtoAdapter>();
            _responseDtoAdapterWithException
                .Setup(mock => mock.Adapt(It.IsAny<GetQueuesResult>()))
                .Throws(new Exception());
        }

        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] // success case
            {
                _requestDtoReader.Object,
                _inputAdapter.Object,
                _dataProvider.Object,
                _responseDtoAdapter.Object,
                (int)HttpStatusCode.OK
            };
            yield return new object[]
            {
                _requestDtoReaderWithException.Object,
                _inputAdapter.Object,
                _dataProvider.Object,
                _responseDtoAdapter.Object,
                (int)HttpStatusCode.InternalServerError
            };
            yield return new object[]
            {
                _requestDtoReader.Object,
                _inputAdapterWithException.Object,
                _dataProvider.Object,
                _responseDtoAdapter.Object,
                (int)HttpStatusCode.InternalServerError
            };
            yield return new object[]
            {
                _requestDtoReader.Object,
                _inputAdapter.Object,
                _dataProviderWithException.Object,
                _responseDtoAdapter.Object,
                (int)HttpStatusCode.InternalServerError
            };
            yield return new object[]
            {
                _requestDtoReader.Object,
                _inputAdapter.Object,
                _dataProvider.Object,
                _responseDtoAdapterWithException.Object,
                (int)HttpStatusCode.InternalServerError
            };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}