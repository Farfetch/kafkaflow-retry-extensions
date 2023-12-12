using System;
using KafkaFlow.Retry.API.Adapters.UpdateQueues;
using KafkaFlow.Retry.Durable.Repository.Actions.Update;
using KafkaFlow.Retry.Durable.Repository.Model;

namespace KafkaFlow.Retry.UnitTests.API.Adapters.UpdateQueues;

public class UpdateQueuesResponseDtoAdapterTests
{
    private readonly IUpdateQueuesResponseDtoAdapter adapter = new UpdateQueuesResponseDtoAdapter();

    [Fact]
    public void UpdateQueuesResponseDtoAdapter_Adapt_Success()
    {
        // Arrange
        var expectedResults = new[]
        {
            new UpdateQueueResult("queueGroupKey1", UpdateQueueResultStatus.NotUpdated, RetryQueueStatus.Active),
            new UpdateQueueResult("queueGroupKey2", UpdateQueueResultStatus.UpdateIsNotAllowed, RetryQueueStatus.Active),
            new UpdateQueueResult("queueGroupKey3", UpdateQueueResultStatus.Updated, RetryQueueStatus.Done)
        };

        var result = new UpdateQueuesResult(expectedResults);

        // Act
        var responseDto = adapter.Adapt(result);

        // Assert
        for (int i = 0; i < responseDto.UpdateQueuesResults.Count; i++)
        {
            responseDto.UpdateQueuesResults[i].QueueGroupKey.Should().Be(expectedResults[i].QueueGroupKey);
            responseDto.UpdateQueuesResults[i].Result.Should().Be(expectedResults[i].Status.ToString());
            responseDto.UpdateQueuesResults[i].QueueStatus.Should().Be(expectedResults[i].RetryQueueStatus.ToString());
        }
    }

    [Fact]
    public void UpdateQueuesResponseDtoAdapter_Adapt_WithNullArgs_ThrowsException()
    {
        // Act
        Action act = () => adapter.Adapt(null);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}