using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KafkaFlow.Retry.API.Adapters.UpdateItems;
using KafkaFlow.Retry.API.Dtos;
using KafkaFlow.Retry.API.Dtos.Common;
using KafkaFlow.Retry.API.Handlers;
using KafkaFlow.Retry.Durable.Repository;
using KafkaFlow.Retry.Durable.Repository.Actions.Update;
using KafkaFlow.Retry.Durable.Repository.Model;
using KafkaFlow.Retry.UnitTests.API.Utilities;
using Microsoft.AspNetCore.Http;
using Moq;

namespace KafkaFlow.Retry.UnitTests.API.Handlers;

public class PatchItemsHandlerTests
{
    private readonly string _httpMethod = "PATCH";
    private readonly Guid _itemId1 = Guid.NewGuid();
    private readonly Guid _itemId2 = Guid.NewGuid();
    private readonly Guid _itemId3 = Guid.NewGuid();
    private readonly string _resourcePath = "/retry/items";

    [Fact]
    public async Task PatchItemsHandler_HandleAsync_Success()
    {
        // Arrange
        var updateItemsRequestDto = CreateRequestDto();

        var httpContext = await HttpContextHelper.CreateContext(_resourcePath, _httpMethod, updateItemsRequestDto);

        var updateItemsInput = CreateInput();
        var updateItemsResult = CreateResult();
        var expectedUpdateItemsResponseDto = CreateResponseDto();

        var mockUpdateItemsInputAdapter = new Mock<IUpdateItemsInputAdapter>();
        mockUpdateItemsInputAdapter
            .Setup(mock => mock.Adapt(It.IsAny<UpdateItemsRequestDto>()))
            .Returns(updateItemsInput);

        var retryQueueDataProvider = new Mock<IRetryDurableQueueRepositoryProvider>();
        retryQueueDataProvider
            .Setup(mock => mock.UpdateItemsAsync(updateItemsInput))
            .ReturnsAsync(updateItemsResult);

        var mockUpdateItemsResponseDtoAdapter = new Mock<IUpdateItemsResponseDtoAdapter>();
        mockUpdateItemsResponseDtoAdapter
            .Setup(mock => mock.Adapt(updateItemsResult))
            .Returns(expectedUpdateItemsResponseDto);

        var handler = new PatchItemsHandler(
            retryQueueDataProvider.Object,
            mockUpdateItemsInputAdapter.Object,
            mockUpdateItemsResponseDtoAdapter.Object,
            string.Empty
        );

        // Act
        var handled = await handler.HandleAsync(httpContext.Request, httpContext.Response);

        // Assert
        handled.Should().BeTrue();
        mockUpdateItemsInputAdapter.Verify(mock => mock.Adapt(It.IsAny<UpdateItemsRequestDto>()), Times.Once());
        retryQueueDataProvider.Verify(mock => mock.UpdateItemsAsync(updateItemsInput), Times.Once());
        mockUpdateItemsResponseDtoAdapter.Verify(mock => mock.Adapt(updateItemsResult), Times.Once());

        var actualResponseDto = await HttpContextHelper.ReadResponse<UpdateItemsResponseDto>(httpContext.Response);
        actualResponseDto.Should().BeEquivalentTo(expectedUpdateItemsResponseDto);
    }

    [Fact]
    public async Task PatchItemsHandler_HandleAsync_WithErrorInDeserialization()
    {
        // arrange
        string expectedDataException = "Newtonsoft.Json.JsonSerializationException";
        var wrongDto = new List<FakeDto> { new FakeDto { DummyProperty = "some text" } };

        var mockHttpContext = HttpContextHelper.MockHttpContext(_resourcePath, _httpMethod, requestBody: wrongDto);

        var httpResponse = new Mock<HttpResponse>();
        string actualData = null;

        _ = httpResponse
            .Setup(_ => _.Body.WriteAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Callback((byte[] data, int _, int length, CancellationToken cancellation) =>
            {
                if (length > 0 && !cancellation.IsCancellationRequested)
                {
                    actualData = Encoding.UTF8.GetString(data);
                }
            })
            .Returns(Task.CompletedTask);

        mockHttpContext
            .SetupGet(ctx => ctx.Response)
            .Returns(httpResponse.Object);

        var handler = new PatchItemsHandler(
            Mock.Of<IRetryDurableQueueRepositoryProvider>(),
            Mock.Of<IUpdateItemsInputAdapter>(),
            Mock.Of<IUpdateItemsResponseDtoAdapter>(),
            string.Empty
        );

        // act
        await handler.HandleAsync(mockHttpContext.Object.Request, mockHttpContext.Object.Response).ConfigureAwait(false);

        // assert
        Assert.Contains(expectedDataException, actualData);
    }

    [Theory]
    [ClassData(typeof(DependenciesThrowingExceptionsData))]
    public async Task PatchItemsHandler_HandleAsync_WithException_ReturnsExpectedStatusCode(
        IUpdateItemsInputAdapter updateItemsInputAdapter,
        IRetryDurableQueueRepositoryProvider retryQueueDataProvider,
        IUpdateItemsResponseDtoAdapter updateItemsResponseDtoAdapter,
        int expectedStatusCode)
    {
        // arrange
        var updateItemsRequestDto = CreateRequestDto();

        var httpContext = await HttpContextHelper.CreateContext(_resourcePath, _httpMethod, updateItemsRequestDto);

        var handler = new PatchItemsHandler(
            retryQueueDataProvider,
            updateItemsInputAdapter,
            updateItemsResponseDtoAdapter,
            string.Empty
        );

        // act
        await handler.HandleAsync(httpContext.Request, httpContext.Response);

        // assert
        Assert.Equal(expectedStatusCode, httpContext.Response.StatusCode);
    }

    private UpdateItemsInput CreateInput()
    {
        return new UpdateItemsInput(
            new[] { _itemId1, _itemId2, _itemId3 },
            RetryQueueItemStatus.Cancelled
        );
    }

    private UpdateItemsRequestDto CreateRequestDto()
    {
        return new UpdateItemsRequestDto
        {
            ItemIds = new[] { _itemId1, _itemId2, _itemId3 },
            Status = RetryQueueItemStatusDto.Cancelled
        };
    }

    private UpdateItemsResponseDto CreateResponseDto()
    {
        return new UpdateItemsResponseDto
        {
            UpdateItemsResults = new[] {
                new UpdateItemResultDto(_itemId1, UpdateItemResultStatus.Updated),
                new UpdateItemResultDto(_itemId2, UpdateItemResultStatus.QueueNotFound),
                new UpdateItemResultDto(_itemId3, UpdateItemResultStatus.ItemIsNotTheFirstWaitingInQueue),
            }
        };
    }

    private UpdateItemsResult CreateResult()
    {
        return new UpdateItemsResult(
            new[] {
                new UpdateItemResult(_itemId1, UpdateItemResultStatus.Updated),
                new UpdateItemResult(_itemId2, UpdateItemResultStatus.QueueNotFound),
                new UpdateItemResult(_itemId3, UpdateItemResultStatus.ItemIsNotInWaitingState)
            });
    }

    internal class FakeDto
    {
        public string DummyProperty { get; set; }
    }

    private class DependenciesThrowingExceptionsData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] // success case
            {
                Mock.Of<IUpdateItemsInputAdapter>(),
                Mock.Of<IRetryDurableQueueRepositoryProvider>(),
                Mock.Of<IUpdateItemsResponseDtoAdapter>(),
                (int)HttpStatusCode.OK
            };
            yield return new object[]
            {
                null,
                Mock.Of<IRetryDurableQueueRepositoryProvider>(),
                Mock.Of<IUpdateItemsResponseDtoAdapter>(),
                (int)HttpStatusCode.InternalServerError
            };
            yield return new object[]
            {
                Mock.Of<IUpdateItemsInputAdapter>(),
                null,
                Mock.Of<IUpdateItemsResponseDtoAdapter>(),
                (int)HttpStatusCode.InternalServerError
            };
            yield return new object[]
            {
                Mock.Of<IUpdateItemsInputAdapter>(),
                Mock.Of<IRetryDurableQueueRepositoryProvider>(),
                null,
                (int)HttpStatusCode.InternalServerError
            };
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}