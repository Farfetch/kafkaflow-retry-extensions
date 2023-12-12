using System;
using System.Collections.Generic;
using KafkaFlow.Retry.Durable.Common;
using KafkaFlow.Retry.Durable.Repository.Model;
using KafkaFlow.Retry.SqlServer.Model;
using KafkaFlow.Retry.SqlServer.Readers;
using KafkaFlow.Retry.SqlServer.Readers.Adapters;
using Moq;

namespace KafkaFlow.Retry.UnitTests.Repositories.SqlServer.Readers;

public class RetryQueueReaderTests
{
    private readonly RetryQueueReader _reader;

    private readonly Mock<IRetryQueueAdapter> _retryQueueAdapter = new();

    private readonly Mock<IRetryQueueItemAdapter> _retryQueueItemAdapter = new();

    private readonly Mock<IRetryQueueItemMessageAdapter> _retryQueueItemMessageAdapter = new();

    private readonly Mock<IRetryQueueItemMessageHeaderAdapter> _retryQueueItemMessageHeaderAdapter = new();

    public RetryQueueReaderTests()
    {
        var item1 = CreateRetryQueueItem(1, RetryQueueItemStatus.InRetry, SeverityLevel.High);
        var itemsA = new[] { item1 };

        _retryQueueAdapter
            .Setup(d => d.Adapt(It.IsAny<RetryQueueDbo>()))
            .Returns(new RetryQueue(Guid.NewGuid(), "searchGroupKeyA", "queueGroupKeyA", DateTime.UtcNow,
                DateTime.UtcNow, RetryQueueStatus.Active, itemsA));

        _retryQueueItemAdapter
            .Setup(d => d.Adapt(It.IsAny<RetryQueueItemDbo>()))
            .Returns(new RetryQueueItem(
                Guid.NewGuid(),
                3,
                DateTime.UtcNow,
                0,
                DateTime.UtcNow,
                DateTime.UtcNow,
                RetryQueueItemStatus.InRetry,
                SeverityLevel.Low,
                "test"));

        _retryQueueItemMessageAdapter
            .Setup(d => d.Adapt(It.IsAny<RetryQueueItemMessageDbo>()))
            .Returns(new RetryQueueItemMessage("topicName", new byte[] { 1, 3 }, new byte[] { 2, 4, 6 }, 3, 21,
                DateTime.UtcNow));

        _retryQueueItemMessageHeaderAdapter
            .Setup(d => d.Adapt(It.IsAny<RetryQueueItemMessageHeaderDbo>()))
            .Returns(new MessageHeader("key", new byte[2]));

        _reader = new RetryQueueReader(
            _retryQueueAdapter.Object,
            _retryQueueItemAdapter.Object,
            _retryQueueItemMessageAdapter.Object,
            _retryQueueItemMessageHeaderAdapter.Object);
    }

    public static IEnumerable<object[]> DataTest()
    {
        return new List<object[]>
        {
            new object[]
            {
                null
            },
            new object[]
            {
                new RetryQueuesDboWrapper
                {
                    HeadersDbos = null,
                    ItemsDbos = new RetryQueueItemDbo[0],
                    MessagesDbos = new RetryQueueItemMessageDbo[0],
                    QueuesDbos = new RetryQueueDbo[0]
                }
            },
            new object[]
            {
                new RetryQueuesDboWrapper
                {
                    HeadersDbos = new RetryQueueItemMessageHeaderDbo[0],
                    ItemsDbos = null,
                    MessagesDbos = new RetryQueueItemMessageDbo[0],
                    QueuesDbos = new RetryQueueDbo[0]
                }
            },
            new object[]
            {
                new RetryQueuesDboWrapper
                {
                    HeadersDbos = new RetryQueueItemMessageHeaderDbo[0],
                    ItemsDbos = new RetryQueueItemDbo[0],
                    MessagesDbos = null,
                    QueuesDbos = new RetryQueueDbo[0]
                }
            },
            new object[]
            {
                new RetryQueuesDboWrapper
                {
                    HeadersDbos = new RetryQueueItemMessageHeaderDbo[0],
                    ItemsDbos = new RetryQueueItemDbo[0],
                    MessagesDbos = new RetryQueueItemMessageDbo[0],
                    QueuesDbos = null
                }
            }
        };
    }

    [Fact]
    public void RetryQueueReader_Read_Success()
    {
        // Arrange
        var wrapper = new RetryQueuesDboWrapper
        {
            QueuesDbos = new[]
            {
                new RetryQueueDbo
                {
                    Id = 1,
                    CreationDate = DateTime.UtcNow,
                    LastExecution = DateTime.UtcNow,
                    Status = RetryQueueStatus.Active,
                    QueueGroupKey = "1",
                    SearchGroupKey = "1"
                }
            },
            ItemsDbos = new[]
            {
                new RetryQueueItemDbo
                {
                    Description = "description",
                    DomainRetryQueueId = Guid.NewGuid(),
                    CreationDate = DateTime.UtcNow,
                    IdDomain = Guid.NewGuid(),
                    ModifiedStatusDate = DateTime.UtcNow,
                    AttemptsCount = 1,
                    Id = 1,
                    LastExecution = DateTime.UtcNow,
                    RetryQueueId = 1,
                    SeverityLevel = SeverityLevel.High,
                    Sort = 1,
                    Status = RetryQueueItemStatus.InRetry
                }
            },
            MessagesDbos = new[]
            {
                new RetryQueueItemMessageDbo
                {
                    IdRetryQueueItem = 1,
                    Key = new byte[] { 1, 3 },
                    Offset = 2,
                    Partition = 1,
                    TopicName = "topicName",
                    UtcTimeStamp = DateTime.UtcNow,
                    Value = new byte[] { 2, 4, 6 }
                }
            },
            HeadersDbos = new[]
            {
                new RetryQueueItemMessageHeaderDbo
                {
                    Id = 1,
                    Key = "key",
                    Value = new byte[2],
                    RetryQueueItemMessageId = 1
                }
            }
        };

        // Act
        var result = _reader.Read(wrapper);

        // Assert
        result.Should().NotBeEmpty();
        result.Count.Should().Be(1);
    }

    [Theory]
    [MemberData(nameof(DataTest))]
    internal void RetryQueueReader_Read_Validation(
        RetryQueuesDboWrapper wrapper)
    {
        // Act
        Action act = () => _reader.Read(wrapper);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    private RetryQueueItem CreateRetryQueueItem(int sort, RetryQueueItemStatus status, SeverityLevel severity)
    {
        return new RetryQueueItem(Guid.NewGuid(), 3, DateTime.UtcNow, sort, DateTime.UtcNow, DateTime.UtcNow, status,
            severity, "description")
        {
            Message = new RetryQueueItemMessage("topicName", new byte[] { 1, 3 }, new byte[] { 2, 4, 6 }, 3, 21,
                DateTime.UtcNow)
        };
    }
}