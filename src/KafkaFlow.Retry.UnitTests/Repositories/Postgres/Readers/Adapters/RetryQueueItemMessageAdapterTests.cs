namespace KafkaFlow.Retry.UnitTests.Repositories.Postgres.Readers.Adapters
{
    using System;
    using FluentAssertions;
    using global::KafkaFlow.Retry.Durable.Repository.Model;
    using global::KafkaFlow.Retry.Postgres.Model;
    using global::KafkaFlow.Retry.Postgres.Readers.Adapters;
    using Xunit;
    
    public class RetryQueueItemMessageAdapterTests
    {
        private readonly RetryQueueItemMessageAdapter adapter = new RetryQueueItemMessageAdapter();

        [Fact]
        public void RetryQueueItemMessageAdapter_Adapt_Success()
        {
            // Arrange
            var retryQueue = new RetryQueueItemMessageDbo
            {
                IdRetryQueueItem = 1,
                Key = new byte[1],
                Offset = 1,
                Partition = 1,
                TopicName = "topic",
                UtcTimeStamp = DateTime.UtcNow,
                Value = new byte[2]
            };

            // Act
            var result = adapter.Adapt(retryQueue);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType(typeof(RetryQueueItemMessage));
        }

        [Fact]
        public void RetryQueueItemMessageAdapter_Adapt_WithoutRetryQueueItemMessageDbo_ThrowsException()
        {
            // Arrange
            RetryQueueItemMessageDbo retryQueue = null;

            // Act
            Action act = () => adapter.Adapt(retryQueue);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }
    }
}