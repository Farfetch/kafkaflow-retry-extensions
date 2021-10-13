namespace KafkaFlow.Retry.UnitTests.API.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using System.Threading.Tasks;
    using FluentAssertions;
    using global::KafkaFlow.Retry.API.Adapters.UpdateQueues;
    using global::KafkaFlow.Retry.API.Dtos;
    using global::KafkaFlow.Retry.API.Dtos.Common;
    using global::KafkaFlow.Retry.API.Handlers;
    using global::KafkaFlow.Retry.Durable.Repository;
    using global::KafkaFlow.Retry.Durable.Repository.Actions.Update;
    using global::KafkaFlow.Retry.Durable.Repository.Model;
    using global::KafkaFlow.Retry.UnitTests.API.Utilities;
    using Moq;
    using Xunit;

    [ExcludeFromCodeCoverage]
    public class PatchQueuesHandlerTests
    {
        private readonly string httpMethod = "PATCH";
        private readonly string resourcePath = "/retry/queues";

        [Fact]
        public async Task PatchQueuesHandler_HandleAsync_Success()
        {
            // Arrange
            var updateQueuesRequestDto = this.CreateRequestDto();

            var httpContext = await HttpContextHelper.CreateContext(this.resourcePath, this.httpMethod, updateQueuesRequestDto);

            var updateQueuesInput = this.CreateInput();
            var updateQueuesResult = this.CreateResult();
            var expectedUpdateQueuesResponseDto = this.CreateResponseDto();

            var mockUpdateQueuesInputAdapter = new Mock<IUpdateQueuesInputAdapter>();
            mockUpdateQueuesInputAdapter
                .Setup(mock => mock.Adapt(It.IsAny<UpdateQueuesRequestDto>()))
                .Returns(updateQueuesInput);

            var retryQueueDataProvider = new Mock<IRetryDurableQueueRepositoryProvider>();
            retryQueueDataProvider
                .Setup(mock => mock.UpdateQueuesAsync(updateQueuesInput))
                .ReturnsAsync(updateQueuesResult);

            var mockUpdateQueuesResponseDtoAdapter = new Mock<IUpdateQueuesResponseDtoAdapter>();
            mockUpdateQueuesResponseDtoAdapter
                .Setup(mock => mock.Adapt(updateQueuesResult))
                .Returns(expectedUpdateQueuesResponseDto);

            var handler = new PatchQueuesHandler(
              retryQueueDataProvider.Object,
              mockUpdateQueuesInputAdapter.Object,
              mockUpdateQueuesResponseDtoAdapter.Object
              );

            // Act
            var handled = await handler.HandleAsync(httpContext.Request, httpContext.Response);

            // Assert
            handled.Should().BeTrue();
            mockUpdateQueuesInputAdapter.Verify(mock => mock.Adapt(It.IsAny<UpdateQueuesRequestDto>()), Times.Once());
            retryQueueDataProvider.Verify(mock => mock.UpdateQueuesAsync(updateQueuesInput), Times.Once());
            mockUpdateQueuesResponseDtoAdapter.Verify(mock => mock.Adapt(updateQueuesResult), Times.Once());

            var actualResponseDto = await HttpContextHelper.ReadResponse<UpdateQueuesResponseDto>(httpContext.Response);
            actualResponseDto.Should().BeEquivalentTo(expectedUpdateQueuesResponseDto);
        }

        [Theory]
        [ClassData(typeof(DependenciesThrowingExceptionsData))]
        public async Task PatchQueuesHandler_HandleAsync_WithException_ReturnsExpectedStatusCode(
        IUpdateQueuesInputAdapter updateQueuesInputAdapter,
        IRetryDurableQueueRepositoryProvider retryQueueDataProvider,
        IUpdateQueuesResponseDtoAdapter updateQueuesResponseDtoAdapter,
        int expectedStatusCode)
        {
            // arrange
            var updateItemsRequestDto = this.CreateRequestDto();

            var httpContext = await HttpContextHelper.CreateContext(this.resourcePath, this.httpMethod, updateItemsRequestDto);

            var handler = new PatchQueuesHandler(
                retryQueueDataProvider,
                updateQueuesInputAdapter,
                updateQueuesResponseDtoAdapter
                );

            // act
            await handler.HandleAsync(httpContext.Request, httpContext.Response);

            // assert
            Assert.Equal(expectedStatusCode, httpContext.Response.StatusCode);
        }

        private UpdateQueuesInput CreateInput()
        {
            return new UpdateQueuesInput(
                new[] { "queueGroupKey1", "queueGroupKey2" },
                RetryQueueItemStatus.Cancelled);
        }

        private UpdateQueuesRequestDto CreateRequestDto()
        {
            return new UpdateQueuesRequestDto
            {
                QueueGroupKeys = new[] { "queueGroupKey1", "queueGroupKey2" },
                ItemStatus = RetryQueueItemStatusDto.Cancelled
            };
        }

        private UpdateQueuesResponseDto CreateResponseDto()
        {
            return new UpdateQueuesResponseDto
            {
                UpdateQueuesResults = new[]
                {
                    new UpdateQueueResultDto("queueGroupKey1", UpdateQueueResultStatus.Updated, RetryQueueStatus.Done)
                }
            };
        }

        private UpdateQueuesResult CreateResult()
        {
            return new UpdateQueuesResult(
                new[]
                {
                    new UpdateQueueResult("queueGroupKey1", UpdateQueueResultStatus.Updated, RetryQueueStatus.Active),
                    new UpdateQueueResult("queueGroupKey2", UpdateQueueResultStatus.NotUpdated, RetryQueueStatus.Active),
                });
        }

        private class DependenciesThrowingExceptionsData : IEnumerable<object[]>
        {
            private readonly Mock<IRetryDurableQueueRepositoryProvider> dataProvider;
            private readonly Mock<IRetryDurableQueueRepositoryProvider> dataProviderWithException;
            private readonly Mock<IUpdateQueuesInputAdapter> inputAdapter;
            private readonly Mock<IUpdateQueuesInputAdapter> inputAdapterWithException;
            private readonly Mock<IUpdateQueuesResponseDtoAdapter> responseDtoAdapter;
            private readonly Mock<IUpdateQueuesResponseDtoAdapter> responseDtoAdapterWithException;

            public DependenciesThrowingExceptionsData()
            {
                this.inputAdapter = new Mock<IUpdateQueuesInputAdapter>();
                this.dataProvider = new Mock<IRetryDurableQueueRepositoryProvider>();
                this.responseDtoAdapter = new Mock<IUpdateQueuesResponseDtoAdapter>();

                this.inputAdapterWithException = new Mock<IUpdateQueuesInputAdapter>();
                inputAdapterWithException
                    .Setup(mock => mock.Adapt(It.IsAny<UpdateQueuesRequestDto>()))
                    .Throws(new Exception());

                this.dataProviderWithException = new Mock<IRetryDurableQueueRepositoryProvider>();
                dataProviderWithException
                    .Setup(mock => mock.UpdateQueuesAsync(It.IsAny<UpdateQueuesInput>()))
                    .ThrowsAsync(new Exception());

                this.responseDtoAdapterWithException = new Mock<IUpdateQueuesResponseDtoAdapter>();
                responseDtoAdapterWithException
                    .Setup(mock => mock.Adapt(It.IsAny<UpdateQueuesResult>()))
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