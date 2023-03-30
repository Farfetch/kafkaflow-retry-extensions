namespace KafkaFlow.Retry.UnitTests.Repositories.Postgres.Readers
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using global::KafkaFlow.Retry.Durable.Common;
    using global::KafkaFlow.Retry.Durable.Repository.Model;
    using global::KafkaFlow.Retry.Postgres.Model;
    using global::KafkaFlow.Retry.Postgres.Readers;
    using global::KafkaFlow.Retry.Postgres.Readers.Adapters;
    using Moq;
    using Xunit;
    
    public class RetryQueueReaderTests
    {
        private readonly RetryQueueReader reader;

        private readonly Mock<IRetryQueueAdapter> retryQueueAdapter = new Mock<IRetryQueueAdapter>();
        
        private readonly Mock<IRetryQueueItemAdapter> retryQueueItemAdapter = new Mock<IRetryQueueItemAdapter>();
        
        private readonly Mock<IRetryQueueItemMessageAdapter> retryQueueItemMessageAdapter = new Mock<IRetryQueueItemMessageAdapter>();
        
        private readonly Mock<IRetryQueueItemMessageHeaderAdapter> retryQueueItemMessageHeaderAdapter = new Mock<IRetryQueueItemMessageHeaderAdapter>();

        public RetryQueueReaderTests()
        {
            var item1 = this.CreateRetryQueueItem(1, RetryQueueItemStatus.InRetry, SeverityLevel.High);
            var itemsA = new[] { item1 };

            retryQueueAdapter
                .Setup(d => d.Adapt(It.IsAny<RetryQueueDbo>()))
                .Returns(new RetryQueue(Guid.NewGuid(), "searchGroupKeyA", "queueGroupKeyA", DateTime.UtcNow, DateTime.UtcNow, RetryQueueStatus.Active, itemsA));

            retryQueueItemAdapter
                .Setup(d => d.Adapt(It.IsAny<RetryQueueItemDbo>()))
                .Returns(new RetryQueueItem(
                            id: Guid.NewGuid(),
                            attemptsCount: 3,
                            creationDate: DateTime.UtcNow,
                            sort: 0,
                            lastExecution: DateTime.UtcNow,
                            modifiedStatusDate: DateTime.UtcNow,
                            status: RetryQueueItemStatus.InRetry,
                            severityLevel: SeverityLevel.Low,
                            description: "test"));

            retryQueueItemMessageAdapter
                .Setup(d => d.Adapt(It.IsAny<RetryQueueItemMessageDbo>()))
                .Returns(new RetryQueueItemMessage("topicName", new byte[] { 1, 3 }, new byte[] { 2, 4, 6 }, 3, 21, DateTime.UtcNow));

            retryQueueItemMessageHeaderAdapter
                .Setup(d => d.Adapt(It.IsAny<RetryQueueItemMessageHeaderDbo>()))
                .Returns(new MessageHeader("key", new byte[2]));

            reader = new RetryQueueReader(
                retryQueueAdapter.Object,
                retryQueueItemAdapter.Object,
                retryQueueItemMessageAdapter.Object,
                retryQueueItemMessageHeaderAdapter.Object);
        }

        public static IEnumerable<object[]> DataTest() => new List<object[]>
        {
            new object[]
            {
                null
            },
            new object[]
            {
                new RetryQueuesDboWrapper
                {
                    HeadersDbos =  null,
                    ItemsDbos =  new RetryQueueItemDbo[0],
                    MessagesDbos = new RetryQueueItemMessageDbo[0],
                    QueuesDbos = new RetryQueueDbo[0]
                }
            },
            new object[]
            {
                new RetryQueuesDboWrapper
                {
                    HeadersDbos = new RetryQueueItemMessageHeaderDbo[0],
                    ItemsDbos =  null,
                    MessagesDbos = new RetryQueueItemMessageDbo[0],
                    QueuesDbos = new RetryQueueDbo[0]
                }
            },
            new object[]
            {
                new RetryQueuesDboWrapper
                {
                    HeadersDbos = new RetryQueueItemMessageHeaderDbo[0],
                    ItemsDbos =  new RetryQueueItemDbo[0],
                    MessagesDbos = null,
                    QueuesDbos = new RetryQueueDbo[0]
                }
            },
            new object[]
            {
                new RetryQueuesDboWrapper
                {
                    HeadersDbos =  new RetryQueueItemMessageHeaderDbo[0],
                    ItemsDbos =  new RetryQueueItemDbo[0],
                    MessagesDbos = new RetryQueueItemMessageDbo[0],
                    QueuesDbos = null
                }
            }
        };

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
                        SeverityLevel = Durable.Common.SeverityLevel.High,
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
                },
            };

            // Act
            var result = reader.Read(wrapper);

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
            Action act = () => reader.Read(wrapper);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        private RetryQueueItem CreateRetryQueueItem(int sort, RetryQueueItemStatus status, SeverityLevel severity)
        {
            return new RetryQueueItem(Guid.NewGuid(), 3, DateTime.UtcNow, sort, DateTime.UtcNow, DateTime.UtcNow, status, severity, "description")
            {
                Message = new RetryQueueItemMessage("topicName", new byte[] { 1, 3 }, new byte[] { 2, 4, 6 }, 3, 21, DateTime.UtcNow)
            };
        }
    }
}