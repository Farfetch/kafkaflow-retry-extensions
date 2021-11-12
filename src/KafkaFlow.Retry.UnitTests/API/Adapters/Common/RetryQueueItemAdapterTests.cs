namespace KafkaFlow.Retry.UnitTests.API.Adapters.Common
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using FluentAssertions;
    using global::KafkaFlow.Retry.API.Adapters.Common;
    using global::KafkaFlow.Retry.Durable.Common;
    using global::KafkaFlow.Retry.Durable.Repository.Model;
    using Xunit;

    [ExcludeFromCodeCoverage]
    public class RetryQueueItemAdapterTests
    {
        private static readonly RetryQueueItem retryQueueItem = new RetryQueueItem(
                            id: Guid.NewGuid(),
                    attemptsCount: 3,
                    creationDate: DateTime.UtcNow,
                    sort: 0,
                    lastExecution: DateTime.UtcNow,
                    modifiedStatusDate: DateTime.UtcNow,
                    status: RetryQueueItemStatus.Waiting,
                    severityLevel: SeverityLevel.Low,
                    description: "test");

        private readonly IRetryQueueItemAdapter adapter = new RetryQueueItemAdapter();

        public static IEnumerable<object[]> DataTest()
        {
            yield return new object[]
            {
                 null
            };
            yield return new object[]
            {
                retryQueueItem
            };
        }

        [Fact]
        public void RetryQueueItemAdapter_Adapt_Success()
        {
            // Arrange
            var expectedGroupKey = "groupKey";

            retryQueueItem.Message = new RetryQueueItemMessage(
                topicName: "topic",
                key: new byte[1],
                value: new byte[1],
                partition: 0,
                offset: 1,
                utcTimeStamp: DateTime.UtcNow
                );

            // Act
            var retryQueueItemDto = this.adapter.Adapt(retryQueueItem, expectedGroupKey);

            // Assert
            retryQueueItemDto.Should().NotBeNull();
            retryQueueItemDto.Should().BeEquivalentTo(retryQueueItem, config =>
                config
                    .Excluding(o => o.ModifiedStatusDate)
                    .Excluding(o => o.Message));
        }

        [Theory]
        [MemberData(nameof(DataTest))]
        public void RetryQueueItemAdapter_Adapt_ThrowsException(RetryQueueItem retryQueueItem)
        {
            // Act
            Action act = () => this.adapter.Adapt(retryQueueItem, string.Empty);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }
    }
}