using KafkaFlow.Retry.API.Adapters.Common;
using KafkaFlow.Retry.API.Dtos.Common;
using KafkaFlow.Retry.Durable.Repository.Model;

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