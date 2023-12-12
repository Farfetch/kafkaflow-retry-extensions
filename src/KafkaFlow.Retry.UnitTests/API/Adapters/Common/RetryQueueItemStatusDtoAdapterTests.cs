using FluentAssertions;
using global::KafkaFlow.Retry.API.Adapters.Common;
using global::KafkaFlow.Retry.API.Dtos.Common;
using global::KafkaFlow.Retry.Durable.Repository.Model;
using Xunit;

namespace KafkaFlow.Retry.UnitTests.API.Adapters.Common;

public class RetryQueueItemStatusDtoAdapterTests
{
    [Theory]
    [InlineData(RetryQueueItemStatusDto.Cancelled, RetryQueueItemStatus.Cancelled)]
    [InlineData(RetryQueueItemStatusDto.Done, RetryQueueItemStatus.Done)]
    [InlineData(RetryQueueItemStatusDto.InRetry, RetryQueueItemStatus.InRetry)]
    [InlineData(RetryQueueItemStatusDto.Waiting, RetryQueueItemStatus.Waiting)]
    [InlineData(RetryQueueItemStatusDto.None, RetryQueueItemStatus.None)]
    public void RetryQueueItemStatusDtoAdapter_Adapt_Success(RetryQueueItemStatusDto dto, RetryQueueItemStatus expectedStatus)
    {
        // Arrange
        var adapter = new RetryQueueItemStatusDtoAdapter();

        // Act
        var actualStatus = adapter.Adapt(dto);

        // Assert
        actualStatus.Should().Be(expectedStatus);
    }
}