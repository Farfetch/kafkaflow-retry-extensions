namespace KafkaFlow.Retry.UnitTests.API.Adapters.UpdateItems
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using FluentAssertions;
    using global::KafkaFlow.Retry.API.Adapters.UpdateItems;
    using global::KafkaFlow.Retry.API.Dtos;
    using global::KafkaFlow.Retry.API.Dtos.Common;
    using Xunit;

    [ExcludeFromCodeCoverage]
    public class UpdateItemsInputAdapterTests
    {
        private readonly IUpdateItemsInputAdapter adapter = new UpdateItemsInputAdapter();

        [Fact]
        public void UpdateItemsInputAdapter_Adapt_Success()
        {
            // Arrange
            var requestDto = new UpdateItemsRequestDto
            {
                ItemIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() },
                Status = RetryQueueItemStatusDto.Cancelled
            };

            // Act
            var input = adapter.Adapt(requestDto);

            // Assert
            input.Should().NotBeNull();
            input.Should().BeEquivalentTo(requestDto);
        }

        [Fact]
        public void UpdateItemsInputAdapter_Adapt_WithNullArgs_ThrowsException()
        {
            // Act
            Action act = () => this.adapter.Adapt(null);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }
    }
}