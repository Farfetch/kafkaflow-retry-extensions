namespace KafkaFlow.Retry.UnitTests.API.Adapters.UpdateQueues
{
    using System;
    using FluentAssertions;
    using global::KafkaFlow.Retry.API.Adapters.UpdateQueues;
    using global::KafkaFlow.Retry.API.Dtos;
    using global::KafkaFlow.Retry.API.Dtos.Common;
    using Xunit;

    public class UpdateQueuesInputAdapterTests
    {
        private readonly IUpdateQueuesInputAdapter adapter = new UpdateQueuesInputAdapter();

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
            var input = adapter.Adapt(requestDto);

            // Assert
            input.Should().NotBeNull();
            input.Should().BeEquivalentTo(requestDto);
        }

        [Fact]
        public void UpdateQueuesInputAdapter_Adapt_WithNullArgs_ThrowsException()
        {
            // Act
            Action act = () => this.adapter.Adapt(null);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }
    }
}