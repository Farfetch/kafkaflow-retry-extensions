namespace KafkaFlow.Retry.UnitTests.Repositories.Postgres.Readers.Adapters
{
    using System;
    using FluentAssertions;
    using global::KafkaFlow.Retry.Durable.Repository.Model;
    using global::KafkaFlow.Retry.Postgres.Model;
    using global::KafkaFlow.Retry.Postgres.Readers.Adapters;
    using Xunit;
    
    public class RetryQueueItemMessageHeaderAdapterTests
    {
        private readonly RetryQueueItemMessageHeaderAdapter adapter = new RetryQueueItemMessageHeaderAdapter();

        [Fact]
        public void RetryQueueItemMessageHeaderAdapter_Adapt_Success()
        {
            // Arrange
            var retryQueue = new RetryQueueItemMessageHeaderDbo
            {
                Id = 1,
                Key = "key",
                Value = new byte[2],
                RetryQueueItemMessageId = 1
            };

            // Act
            var result = adapter.Adapt(retryQueue);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType(typeof(MessageHeader));
        }

        [Fact]
        public void RetryQueueItemMessageHeaderAdapter_Adapt_WithoutRetryQueueItemMessageHeaderDbo_ThrowsException()
        {
            // Arrange
            RetryQueueItemMessageHeaderDbo retryQueue = null;

            // Act
            Action act = () => adapter.Adapt(retryQueue);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }
    }
}