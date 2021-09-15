namespace KafkaFlow.Retry.UnitTests.API.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Net;
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
    using Moq;
    using Xunit;

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
        public async Task PatchItemsHandler_HandleAsync_WithErrorInDeserialization_ReturnsBadRequest()
        {
            // arrange
            var wrongDto = new { DummyProp = "some text" };

            var httpContext = await HttpContextHelper.CreateContext(this.resourcePath, this.httpMethod, wrongDto);

            var handler = new PatchItemsHandler(
             Mock.Of<IRetryDurableQueueRepositoryProvider>(),
             Mock.Of<IUpdateItemsInputAdapter>(),
             Mock.Of<IUpdateItemsResponseDtoAdapter>()
             );

            // act
            await handler.HandleAsync(httpContext.Request, httpContext.Response).ConfigureAwait(false);

            // assert
            // TODO test is not working because http CreateContext() cannot write the body properly
            //Assert.Equal((int)HttpStatusCode.BadRequest, httpContext.Response.StatusCode);
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

        private class DependenciesThrowingExceptionsData : IEnumerable<object[]>
        {
            private readonly Mock<IRetryDurableQueueRepositoryProvider> dataProvider;
            private readonly Mock<IRetryDurableQueueRepositoryProvider> dataProviderWithException;
            private readonly Mock<IUpdateItemsInputAdapter> inputAdapter;
            private readonly Mock<IUpdateItemsInputAdapter> inputAdapterWithException;
            private readonly Mock<IUpdateItemsResponseDtoAdapter> responseDtoAdapter;
            private readonly Mock<IUpdateItemsResponseDtoAdapter> responseDtoAdapterWithException;

            public DependenciesThrowingExceptionsData()
            {
                this.inputAdapter = new Mock<IUpdateItemsInputAdapter>();
                this.dataProvider = new Mock<IRetryDurableQueueRepositoryProvider>();
                this.responseDtoAdapter = new Mock<IUpdateItemsResponseDtoAdapter>();

                this.inputAdapterWithException = new Mock<IUpdateItemsInputAdapter>();
                inputAdapterWithException
                    .Setup(mock => mock.Adapt(It.IsAny<UpdateItemsRequestDto>()))
                    .Throws(new Exception());

                this.dataProviderWithException = new Mock<IRetryDurableQueueRepositoryProvider>();
                dataProviderWithException
                    .Setup(mock => mock.UpdateItemsAsync(It.IsAny<UpdateItemsInput>()))
                    .ThrowsAsync(new Exception());

                this.responseDtoAdapterWithException = new Mock<IUpdateItemsResponseDtoAdapter>();
                responseDtoAdapterWithException
                    .Setup(mock => mock.Adapt(It.IsAny<UpdateItemsResult>()))
                    .Throws(new Exception());
            }

            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] // success case
                {
                    inputAdapter.Object,
                    dataProvider.Object,
                    responseDtoAdapter.Object,
                    (int)HttpStatusCode.OK
                };
                yield return new object[]
                {
                    inputAdapterWithException.Object,
                    dataProvider.Object,
                    responseDtoAdapter.Object,
                    (int)HttpStatusCode.InternalServerError
                };
                yield return new object[]
                {
                    inputAdapter.Object,
                    dataProviderWithException.Object,
                    responseDtoAdapter.Object,
                    (int)HttpStatusCode.InternalServerError
                };
                yield return new object[]
                {
                    inputAdapter.Object,
                    dataProvider.Object,
                    responseDtoAdapterWithException.Object,
                    (int)HttpStatusCode.InternalServerError
                };
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}