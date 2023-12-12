using System;
using System.Collections.Generic;
using KafkaFlow.Retry.API.Adapters.Common;
using KafkaFlow.Retry.Durable.Common;
using KafkaFlow.Retry.Durable.Repository.Model;

namespace KafkaFlow.Retry.UnitTests.API.Adapters.Common;

public class RetryQueueItemAdapterTests
{
    private static readonly RetryQueueItem s_retryQueueItem = new RetryQueueItem(
        id: Guid.NewGuid(),
        attemptsCount: 3,
        creationDate: DateTime.UtcNow,
        sort: 0,
        lastExecution: DateTime.UtcNow,
        modifiedStatusDate: DateTime.UtcNow,
        status: RetryQueueItemStatus.Waiting,
        severityLevel: SeverityLevel.Low,
        description: "test");

    private readonly IRetryQueueItemAdapter _adapter = new RetryQueueItemAdapter();

    public static IEnumerable<object[]> DataTest()
    {
        yield return new object[]
        {
            null
        };
        yield return new object[]
        {
            s_retryQueueItem
        };
    }

    [Fact]
    public void RetryQueueItemAdapter_Adapt_Success()
    {
        // Arrange
        var expectedGroupKey = "groupKey";

        s_retryQueueItem.Message = new RetryQueueItemMessage(
            topicName: "topic",
            key: new byte[1],
            value: new byte[1],
            partition: 0,
            offset: 1,
            utcTimeStamp: DateTime.UtcNow
        );

        // Act
        var retryQueueItemDto = _adapter.Adapt(s_retryQueueItem, expectedGroupKey);

        // Assert
        retryQueueItemDto.Should().NotBeNull();
        retryQueueItemDto.Should().BeEquivalentTo(s_retryQueueItem, config =>
            config
                .Excluding(o => o.ModifiedStatusDate)
                .Excluding(o => o.Message));
    }

    [Theory]
    [MemberData(nameof(DataTest))]
    public void RetryQueueItemAdapter_Adapt_ThrowsException(RetryQueueItem retryQueueItem)
    {
        // Act
        Action act = () => _adapter.Adapt(retryQueueItem, string.Empty);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}