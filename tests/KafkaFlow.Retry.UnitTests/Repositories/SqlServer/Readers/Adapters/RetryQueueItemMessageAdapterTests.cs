﻿using System;
using KafkaFlow.Retry.Durable.Repository.Model;
using KafkaFlow.Retry.SqlServer.Model;
using KafkaFlow.Retry.SqlServer.Readers.Adapters;

namespace KafkaFlow.Retry.UnitTests.Repositories.SqlServer.Readers.Adapters;

public class RetryQueueItemMessageAdapterTests
{
    private readonly RetryQueueItemMessageAdapter _adapter = new();

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
        var result = _adapter.Adapt(retryQueue);

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
        Action act = () => _adapter.Adapt(retryQueue);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}