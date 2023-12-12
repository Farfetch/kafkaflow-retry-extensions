using System;
using System.Collections.Generic;
using KafkaFlow.Retry.Durable.Repository.Model;
using KafkaFlow.Retry.MongoDb.Adapters;
using KafkaFlow.Retry.MongoDb.Adapters.Interfaces;
using KafkaFlow.Retry.MongoDb.Model;
using Moq;

namespace KafkaFlow.Retry.UnitTests.Repositories.MongoDb.Adapters;

public class MessageAdapterTests
{
    private readonly Mock<IHeaderAdapter> headerAdapter = new Mock<IHeaderAdapter>();

    public MessageAdapterTests()
    {
            headerAdapter.Setup(d => d.Adapt(It.IsAny<MessageHeader>())).Returns(new RetryQueueHeaderDbo());
            headerAdapter.Setup(d => d.Adapt(It.IsAny<RetryQueueHeaderDbo>())).Returns(new MessageHeader("key", new byte[1]));
        }

    [Fact]
    public void MessageAdapter_Adapt_WithoutRetryQueueItemMessage_ThrowException()
    {
            //Arrange
            var adapter = new MessageAdapter(headerAdapter.Object);
            RetryQueueItemMessage retryQueueItemMessage = null;

            // Act
            Action act = () => adapter.Adapt(retryQueueItemMessage);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

    [Fact]
    public void MessageAdapter_Adapt_WithoutRetryQueueItemMessageDbo_ThrowException()
    {
            //Arrange
            var adapter = new MessageAdapter(headerAdapter.Object);
            RetryQueueItemMessageDbo retryQueueItemDbo = null;

            // Act
            Action act = () => adapter.Adapt(retryQueueItemDbo);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

    [Fact]
    public void MessageAdapter_Adapt_WithRetryQueueItemMessage_Success()
    {
            //Arrange
            var adapter = new MessageAdapter(headerAdapter.Object);
            var retryQueueItemMessage = new RetryQueueItemMessage("topicName", new byte[] { 1, 3 }, new byte[] { 2, 4, 6 }, 3, 21, DateTime.UtcNow);

            // Act
            var result = adapter.Adapt(retryQueueItemMessage);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType(typeof(RetryQueueItemMessageDbo));
        }

    [Fact]
    public void MessageAdapter_Adapt_WithRetryQueueItemMessageDbo_Success()
    {
            //Arrange
            var adapter = new MessageAdapter(headerAdapter.Object);
            var retryQueueItemDbo = new RetryQueueItemMessageDbo
            {
                Headers = new List<RetryQueueHeaderDbo>
                {
                    new RetryQueueHeaderDbo()
                },
                Key = new byte[] { 1, 3 },
                Offset = 2,
                Partition = 1,
                TopicName = "topicName",
                UtcTimeStamp = DateTime.UtcNow,
                Value = new byte[] { 2, 4, 6 }
            };

            // Act
            var result = adapter.Adapt(retryQueueItemDbo);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType(typeof(RetryQueueItemMessage));
        }

    [Fact]
    public void MessageAdapter_Ctor_WithoutHeaderAdapter_ThrowException()
    {
            // Act
            Action act = () => new MessageAdapter(null);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }
}