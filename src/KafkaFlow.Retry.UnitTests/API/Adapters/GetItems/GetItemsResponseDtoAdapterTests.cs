using System;
using KafkaFlow.Retry.API.Adapters.GetItems;
using KafkaFlow.Retry.Durable.Common;
using KafkaFlow.Retry.Durable.Repository.Actions.Read;
using KafkaFlow.Retry.Durable.Repository.Model;

namespace KafkaFlow.Retry.UnitTests.API.Adapters.GetItems;

public class GetItemsResponseDtoAdapterTests
{
    private readonly IGetItemsResponseDtoAdapter adapter = new GetItemsResponseDtoAdapter();

    [Fact]
    public void GetItemsResponseDtoAdapter_Adapt_Success()
    {
            // Arrange
            var item1 = CreateRetryQueueItem(1, RetryQueueItemStatus.InRetry, SeverityLevel.High);
            var item2 = CreateRetryQueueItem(2, RetryQueueItemStatus.Waiting, SeverityLevel.High);
            var itemsA = new[] { item1, item2 };

            var item3 = CreateRetryQueueItem(1, RetryQueueItemStatus.Waiting, SeverityLevel.Medium);
            var item4 = CreateRetryQueueItem(2, RetryQueueItemStatus.Waiting, SeverityLevel.Medium);
            var itemsB = new[] { item3, item4 };

            var queueA = new RetryQueue(Guid.NewGuid(), "searchGroupKeyA", "queueGroupKeyA", DateTime.UtcNow, DateTime.UtcNow, RetryQueueStatus.Active, itemsA);
            var queueB = new RetryQueue(Guid.NewGuid(), "searchGroupKeyB", "queueGroupKeyB", DateTime.UtcNow, DateTime.UtcNow, RetryQueueStatus.Done, itemsB);

            var queues = new[] { queueA, queueB };

            var expectedRetryQueueItems = new[] { item1, item2, item3, item4 };

            var getQueuesResult = new GetQueuesResult(queues);

            // Act
            var responseDto = adapter.Adapt(getQueuesResult);

            // Assert
            responseDto.Should().NotBeNull();
            responseDto.QueueItems.Should().BeEquivalentTo(expectedRetryQueueItems, options => options.ExcludingMissingMembers());
        }

    [Fact]
    public void GetItemsResponseDtoAdapter_Adapt_WithNullArgs_ThrowsException()
    {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => adapter.Adapt(null));
        }

    private RetryQueueItem CreateRetryQueueItem(int sort, RetryQueueItemStatus status, SeverityLevel severity)
    {
            return new RetryQueueItem(Guid.NewGuid(), 3, DateTime.UtcNow, sort, DateTime.UtcNow, DateTime.UtcNow, status, severity, "description")
            {
                Message = new RetryQueueItemMessage("topicName", new byte[] { 1, 3 }, new byte[] { 2, 4, 6 }, 3, 21, DateTime.UtcNow)
            };
        }
}