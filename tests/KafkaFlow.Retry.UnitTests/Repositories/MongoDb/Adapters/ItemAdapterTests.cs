﻿using System;
using System.Collections.Generic;
using KafkaFlow.Retry.Durable.Common;
using KafkaFlow.Retry.Durable.Repository.Model;
using KafkaFlow.Retry.MongoDb.Adapters;
using KafkaFlow.Retry.MongoDb.Adapters.Interfaces;
using KafkaFlow.Retry.MongoDb.Model;
using Moq;

namespace KafkaFlow.Retry.UnitTests.Repositories.MongoDb.Adapters;

public class ItemAdapterTests
{
    private readonly Mock<IMessageAdapter> _messageAdapter = new();

    public ItemAdapterTests()
    {
        var retryQueueItemMessage = new RetryQueueItemMessage("topicName", new byte[] { 1, 3 }, new byte[] { 2, 4, 6 },
            3, 21, DateTime.UtcNow);
        _messageAdapter.Setup(d => d.Adapt(It.IsAny<RetryQueueItemMessageDbo>())).Returns(retryQueueItemMessage);
    }

    [Fact]
    public void HeaderAdapter_Adapt_WithMessageHeader_Success()
    {
        //Arrange
        var adapter = new ItemAdapter(_messageAdapter.Object);
        var retryQueueItemDbo = new RetryQueueItemDbo
        {
            Status = RetryQueueItemStatus.InRetry,
            Description = "description",
            CreationDate = DateTime.UtcNow,
            ModifiedStatusDate = DateTime.UtcNow,
            AttemptsCount = 1,
            Id = Guid.NewGuid(),
            LastExecution = DateTime.UtcNow,
            Message = new RetryQueueItemMessageDbo
            {
                Headers = new List<RetryQueueHeaderDbo>
                {
                    new()
                },
                Key = new byte[] { 1, 3 },
                Offset = 2,
                Partition = 1,
                TopicName = "topicName",
                UtcTimeStamp = DateTime.UtcNow,
                Value = new byte[] { 2, 4, 6 }
            },
            RetryQueueId = Guid.NewGuid(),
            SeverityLevel = SeverityLevel.High,
            Sort = 0
        };

        // Act
        var result = adapter.Adapt(retryQueueItemDbo);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType(typeof(RetryQueueItem));
    }

    [Fact]
    public void HeaderAdapter_Adapt_WithoutMessageHeader_ThrowException()
    {
        //Arrange
        var adapter = new ItemAdapter(_messageAdapter.Object);
        RetryQueueItemDbo retryQueueItemDbo = null;

        // Act
        Action act = () => adapter.Adapt(retryQueueItemDbo);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void HeaderAdapter_Ctro_WithoutMessageAdapter_ThrowException()
    {
        // Act
        Action act = () => new ItemAdapter(null);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}