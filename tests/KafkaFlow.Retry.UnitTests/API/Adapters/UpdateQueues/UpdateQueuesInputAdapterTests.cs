using System;
using KafkaFlow.Retry.API.Adapters.UpdateQueues;
using KafkaFlow.Retry.API.Dtos;
using KafkaFlow.Retry.API.Dtos.Common;

namespace KafkaFlow.Retry.UnitTests.API.Adapters.UpdateQueues;

public class UpdateQueuesInputAdapterTests
{
    private readonly IUpdateQueuesInputAdapter _adapter = new UpdateQueuesInputAdapter();

    [Fact]
    public void UpdateQueuesInputAdapter_Adapt_Success()
    {
            // Arrange
            var requestDto = new UpdateQueuesRequestDto
            {
                QueueGroupKeys = new[] { "queueOrderKey1", "queueOrderKey2", "queueOrderKey3" },
                ItemStatus = RetryQueueItemStatusDto.Cancelled
            };

            // Act
            var input = _adapter.Adapt(requestDto);

            // Assert
            input.Should().NotBeNull();
            input.Should().BeEquivalentTo(requestDto);
        }

    [Fact]
    public void UpdateQueuesInputAdapter_Adapt_WithNullArgs_ThrowsException()
    {
            // Act
            Action act = () => _adapter.Adapt(null);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }
}