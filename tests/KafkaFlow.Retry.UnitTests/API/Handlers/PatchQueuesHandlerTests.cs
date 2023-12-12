using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using KafkaFlow.Retry.API.Adapters.UpdateQueues;
using KafkaFlow.Retry.API.Dtos;
using KafkaFlow.Retry.API.Dtos.Common;
using KafkaFlow.Retry.API.Handlers;
using KafkaFlow.Retry.Durable.Repository;
using KafkaFlow.Retry.Durable.Repository.Actions.Update;
using KafkaFlow.Retry.Durable.Repository.Model;
using KafkaFlow.Retry.UnitTests.API.Utilities;
using Moq;

namespace KafkaFlow.Retry.UnitTests.API.Handlers;

public class PatchQueuesHandlerTests
{
    private readonly string _httpMethod = "PATCH";
    private readonly string _resourcePath = "/retry/queues";

    [Fact]
    public async Task PatchQueuesHandler_HandleAsync_Success()
    {
            // Arrange
            var updateQueuesRequestDto = CreateRequestDto();

            var httpContext = await HttpContextHelper.CreateContext(_resourcePath, _httpMethod, updateQueuesRequestDto);

            var updateQueuesInput = CreateInput();
            var updateQueuesResult = CreateResult();
            var expectedUpdateQueuesResponseDto = CreateResponseDto();

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
              mockUpdateQueuesResponseDtoAdapter.Object,
              string.Empty
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
            var updateItemsRequestDto = CreateRequestDto();

            var httpContext = await HttpContextHelper.CreateContext(_resourcePath, _httpMethod, updateItemsRequestDto);

            var handler = new PatchQueuesHandler(
                retryQueueDataProvider,
                updateQueuesInputAdapter,
                updateQueuesResponseDtoAdapter,
                string.Empty
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
        private readonly Mock<IRetryDurableQueueRepositoryProvider> _dataProvider;
        private readonly Mock<IRetryDurableQueueRepositoryProvider> _dataProviderWithException;
        private readonly Mock<IUpdateQueuesInputAdapter> _inputAdapter;
        private readonly Mock<IUpdateQueuesInputAdapter> _inputAdapterWithException;
        private readonly Mock<IUpdateQueuesResponseDtoAdapter> _responseDtoAdapter;
        private readonly Mock<IUpdateQueuesResponseDtoAdapter> _responseDtoAdapterWithException;

        public DependenciesThrowingExceptionsData()
        {
                _inputAdapter = new Mock<IUpdateQueuesInputAdapter>();
                _dataProvider = new Mock<IRetryDurableQueueRepositoryProvider>();
                _responseDtoAdapter = new Mock<IUpdateQueuesResponseDtoAdapter>();

                _inputAdapterWithException = new Mock<IUpdateQueuesInputAdapter>();
                _inputAdapterWithException
                    .Setup(mock => mock.Adapt(It.IsAny<UpdateQueuesRequestDto>()))
                    .Throws(new Exception());

                _dataProviderWithException = new Mock<IRetryDurableQueueRepositoryProvider>();
                _dataProviderWithException
                    .Setup(mock => mock.UpdateQueuesAsync(It.IsAny<UpdateQueuesInput>()))
                    .ThrowsAsync(new Exception());

                _responseDtoAdapterWithException = new Mock<IUpdateQueuesResponseDtoAdapter>();
                _responseDtoAdapterWithException
                    .Setup(mock => mock.Adapt(It.IsAny<UpdateQueuesResult>()))
                    .Throws(new Exception());
            }

        public IEnumerator<object[]> GetEnumerator()
        {
                yield return new object[] // success case
                {
                    _inputAdapter.Object,
                    _dataProvider.Object,
                    _responseDtoAdapter.Object,
                    (int)HttpStatusCode.OK
                };
                yield return new object[]
                {
                    _inputAdapterWithException.Object,
                    _dataProvider.Object,
                    _responseDtoAdapter.Object,
                    (int)HttpStatusCode.InternalServerError
                };
                yield return new object[]
                {
                    _inputAdapter.Object,
                    _dataProviderWithException.Object,
                    _responseDtoAdapter.Object,
                    (int)HttpStatusCode.InternalServerError
                };
                yield return new object[]
                {
                    _inputAdapter.Object,
                    _dataProvider.Object,
                    _responseDtoAdapterWithException.Object,
                    (int)HttpStatusCode.InternalServerError
                };
            }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}