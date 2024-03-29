﻿using System;
using KafkaFlow.Retry.Durable.Repository.Model;
using KafkaFlow.Retry.Postgres.Model;
using KafkaFlow.Retry.Postgres.Readers.Adapters;

namespace KafkaFlow.Retry.UnitTests.Repositories.Postgres.Readers.Adapters;

public class RetryQueueItemMessageHeaderAdapterTests
{
    private readonly RetryQueueItemMessageHeaderAdapter _adapter = new();

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
        var result = _adapter.Adapt(retryQueue);

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
        Action act = () => _adapter.Adapt(retryQueue);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}