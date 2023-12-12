﻿using System;
using KafkaFlow.Retry.Durable.Repository.Model;
using KafkaFlow.Retry.SqlServer.Model;
using KafkaFlow.Retry.SqlServer.Readers.Adapters;

namespace KafkaFlow.Retry.UnitTests.Repositories.SqlServer.Readers.Adapters;

public class RetryQueueItemAdapterTests
{
    private readonly RetryQueueItemAdapter _adapter = new RetryQueueItemAdapter();

    [Fact]
    public void RetryQueueItemAdapter_Adapt_Success()
    {
            // Arrange
            var retryQueue = new RetryQueueItemDbo
            {
                CreationDate = DateTime.UtcNow,
                IdDomain = Guid.NewGuid(),
                Id = 1,
                LastExecution = DateTime.UtcNow,
                Description = "description",
                DomainRetryQueueId = Guid.NewGuid(),
                ModifiedStatusDate = DateTime.UtcNow,
                AttemptsCount = 1,
                RetryQueueId = 1,
                SeverityLevel = Durable.Common.SeverityLevel.High,
                Sort = 1,
                Status = RetryQueueItemStatus.InRetry
            };

            // Act
            var result = _adapter.Adapt(retryQueue);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType(typeof(RetryQueueItem));
        }

    [Fact]
    public void RetryQueueItemAdapter_Adapt_WithoutRetryQueueItemDbo_ThrowsException()
    {
            // Arrange
            RetryQueueItemDbo retryQueue = null;

            // Act
            Action act = () => _adapter.Adapt(retryQueue);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }
}