﻿using System;
using KafkaFlow.Retry.API.Adapters.GetItems;
using KafkaFlow.Retry.API.Dtos;
using KafkaFlow.Retry.Durable.Common;
using KafkaFlow.Retry.Durable.Repository.Model;

namespace KafkaFlow.Retry.UnitTests.API.Adapters.GetItems;

public class GetItemsInputAdapterTests
{
    private readonly IGetItemsInputAdapter adapter = new GetItemsInputAdapter();

    [Fact]
    public void GetItemsInputAdapter_Adapt_Success()
    {
        // Arrange
        var requestDto = new GetItemsRequestDto
        {
            ItemsStatuses = new RetryQueueItemStatus[] { RetryQueueItemStatus.Waiting, RetryQueueItemStatus.InRetry },
            SeverityLevels = new SeverityLevel[] { SeverityLevel.High, SeverityLevel.Medium },
            TopQueues = 1000,
            TopItemsByQueue = 100
        };

        // Act
        var queuesInput = adapter.Adapt(requestDto);

        // Assert
        queuesInput.Should().BeEquivalentTo(requestDto);

        queuesInput.SearchGroupKey.Should().BeNull();
    }

    [Fact]
    public void GetItemsInputAdapter_Adapt_WithNullArg_ThrowsException()
    {
        // Act
        Action act = () => adapter.Adapt(null);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}