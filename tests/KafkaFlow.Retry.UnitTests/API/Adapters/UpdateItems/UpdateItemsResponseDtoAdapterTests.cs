﻿using System;
using KafkaFlow.Retry.API.Adapters.UpdateItems;
using KafkaFlow.Retry.Durable.Repository.Actions.Update;

namespace KafkaFlow.Retry.UnitTests.API.Adapters.UpdateItems;

public class UpdateItemsResponseDtoAdapterTests
{
    private readonly IUpdateItemsResponseDtoAdapter _adapter = new UpdateItemsResponseDtoAdapter();

    [Fact]
    public void UpdateItemsResponseDtoAdapter_Adapt_Success()
    {
        // Arrange
        var expectedResults = new[]
        {
            new UpdateItemResult(Guid.NewGuid(), UpdateItemResultStatus.ItemIsNotInWaitingState),
            new UpdateItemResult(Guid.NewGuid(), UpdateItemResultStatus.UpdateIsNotAllowed),
            new UpdateItemResult(Guid.NewGuid(), UpdateItemResultStatus.Updated)
        };

        var updateItemsResult = new UpdateItemsResult(expectedResults);

        // Act
        var responseDto = _adapter.Adapt(updateItemsResult);

        // Assert
        for (var i = 0; i < responseDto.UpdateItemsResults.Count; i++)
        {
            responseDto.UpdateItemsResults[i].ItemId.Should().Be(expectedResults[i].Id);
            responseDto.UpdateItemsResults[i].Result.Should().Be(expectedResults[i].Status.ToString());
        }
    }

    [Fact]
    public void UpdateItemsResponseDtoAdapter_Adapt_WithNullArgs_ThrowsException()
    {
        // Act
        Action act = () => _adapter.Adapt(null);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}