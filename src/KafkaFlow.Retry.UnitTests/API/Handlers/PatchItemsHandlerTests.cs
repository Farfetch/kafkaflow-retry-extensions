namespace KafkaFlow.Retry.UnitTests.API.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using global::KafkaFlow.Retry.API.Adapters.UpdateItems;
    using global::KafkaFlow.Retry.API.Dtos;
    using global::KafkaFlow.Retry.API.Dtos.Common;
    using global::KafkaFlow.Retry.API.Handlers;
    using global::KafkaFlow.Retry.Durable.Repository;
    using global::KafkaFlow.Retry.Durable.Repository.Actions.Update;
    using global::KafkaFlow.Retry.Durable.Repository.Model;
    using global::KafkaFlow.Retry.UnitTests.API.Utilities;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using Xunit;

    [ExcludeFromCodeCoverage]
    public class PatchItemsHandlerTests
    {
        private readonly string httpMethod = "PATCH";
        private readonly Guid itemId1 = Guid.NewGuid();
        private readonly Guid itemId2 = Guid.NewGuid();
        private readonly Guid itemId3 = Guid.NewGuid();
        private readonly string resourcePath = "/retry/items";

        [Fact]
        public async Task PatchItemsHandler_HandleAsync_Success()
        {
            // Arrange
            var updateItemsRequestDto = this.CreateRequestDto();

            var httpContext = await HttpContextHelper.CreateContext(this.resourcePath, this.httpMethod, updateItemsRequestDto);

            var updateItemsInput = this.CreateInput();
            var updateItemsResult = this.CreateResult();
            var expectedUpdateItemsResponseDto = this.CreateResponseDto();

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
                mockUpdateItemsResponseDtoAdapter.Object
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

            var mockHttpContext = HttpContextHelper.MockHttpContext(this.resourcePath, this.httpMethod, requestBody: wrongDto);

            var httpResponse = new Mock<HttpResponse>();
            string actualData = null;

            httpResponse
                .Setup(_ => _.Body.WriteAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Callback((byte[] data, int offset, int length, CancellationToken token) =>
                {
                    if (length > 0)
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
             Mock.Of<IUpdateItemsResponseDtoAdapter>()
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
            var updateItemsRequestDto = this.CreateRequestDto();

            var httpContext = await HttpContextHelper.CreateContext(this.resourcePath, this.httpMethod, updateItemsRequestDto);

            var handler = new PatchItemsHandler(
                retryQueueDataProvider,
                updateItemsInputAdapter,
                updateItemsResponseDtoAdapter
                );

            // act
            await handler.HandleAsync(httpContext.Request, httpContext.Response);

            // assert
            Assert.Equal(expectedStatusCode, httpContext.Response.StatusCode);
        }

        private UpdateItemsInput CreateInput()
        {
            return new UpdateItemsInput(
                new Guid[] { itemId1, itemId2, itemId3 },
                RetryQueueItemStatus.Cancelled
                );
        }

        private UpdateItemsRequestDto CreateRequestDto()
        {
            return new UpdateItemsRequestDto()
            {
                ItemIds = new Guid[] { itemId1, itemId2, itemId3 },
                Status = RetryQueueItemStatusDto.Cancelled
            };
        }

        private UpdateItemsResponseDto CreateResponseDto()
        {
            return new UpdateItemsResponseDto()
            {
                UpdateItemsResults = new UpdateItemResultDto[] {
                    new UpdateItemResultDto(itemId1, UpdateItemResultStatus.Updated),
                    new UpdateItemResultDto(itemId2, UpdateItemResultStatus.QueueNotFound),
                    new UpdateItemResultDto(itemId3, UpdateItemResultStatus.ItemIsNotTheFirstWaitingInQueue),
                }
            };
        }

        private UpdateItemsResult CreateResult()
        {
            return new UpdateItemsResult(
                new UpdateItemResult[] {
                    new UpdateItemResult(itemId1, UpdateItemResultStatus.Updated),
                    new UpdateItemResult(itemId2, UpdateItemResultStatus.QueueNotFound),
                    new UpdateItemResult(itemId3, UpdateItemResultStatus.ItemIsNotInWaitingState)
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
}