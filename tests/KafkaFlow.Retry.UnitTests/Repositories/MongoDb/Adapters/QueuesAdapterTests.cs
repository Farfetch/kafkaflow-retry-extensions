using System;
using System.Collections.Generic;
using KafkaFlow.Retry.Durable;
using KafkaFlow.Retry.Durable.Common;
using KafkaFlow.Retry.Durable.Repository.Model;
using KafkaFlow.Retry.MongoDb.Adapters;
using KafkaFlow.Retry.MongoDb.Adapters.Interfaces;
using KafkaFlow.Retry.MongoDb.Model;
using Moq;

namespace KafkaFlow.Retry.UnitTests.Repositories.MongoDb.Adapters;

public class QueuesAdapterTests
{
    [Fact]
    public void Adapt_SameSortOnDiffItems_ThrowsException()
    {
        //Arrange
        var retryQueueId = Guid.Parse("A278590F-299B-4F4C-88F0-1EA3C4588786");

        var mockIItemAdapter = new Mock<IItemAdapter>();
        mockIItemAdapter
            .Setup(x => x.Adapt(It.IsAny<RetryQueueItemDbo>()))
            .Returns(
                new Queue<RetryQueueItem>(new List<RetryQueueItem>
                {
                    new RetryQueueItem(Guid.Parse("A42311CF-2156-4A0C-AD81-EA235AA31B79"),1,DateTime.UtcNow,0,null,null,RetryQueueItemStatus.Waiting,SeverityLevel.Unknown,"1"),
                    new RetryQueueItem(Guid.Parse("131DEACB-47D5-41BE-9755-9D8C7ED5576B"),1,DateTime.UtcNow,1,null,null,RetryQueueItemStatus.Waiting,SeverityLevel.Unknown,"2"),
                    new RetryQueueItem(Guid.Parse("4DF0634D-3207-485D-9C5E-100BFF4607C5"),1,DateTime.UtcNow,1,null,null,RetryQueueItemStatus.Waiting,SeverityLevel.Unknown,"3"),
                    new RetryQueueItem(Guid.Parse("7A1F3DA3-EFFD-46EA-A476-E60881246D5E"),1,DateTime.UtcNow,2,null,null,RetryQueueItemStatus.Waiting,SeverityLevel.Unknown,"4"),
                    new RetryQueueItem(Guid.Parse("43220795-5BF1-4023-ADA1-789A391B0997"),1,DateTime.UtcNow,3,null,null,RetryQueueItemStatus.Waiting,SeverityLevel.Unknown,"5"),
                }).Dequeue);

        var adapter = new QueuesAdapter(mockIItemAdapter.Object);

        IEnumerable<RetryQueueDbo> queuesDbo = new List<RetryQueueDbo>
        {
            new RetryQueueDbo
            {
                Id = retryQueueId,
                CreationDate = DateTime.UtcNow,
                LastExecution = DateTime.UtcNow,
                QueueGroupKey = "QueueGroupKey",
                SearchGroupKey = "SearchGroupKey",
                Status = RetryQueueStatus.Active,
            }
        };
        IEnumerable<RetryQueueItemDbo> itemsDbo = new List<RetryQueueItemDbo>
        {
            new RetryQueueItemDbo { RetryQueueId = retryQueueId },
            new RetryQueueItemDbo { RetryQueueId = retryQueueId },
            new RetryQueueItemDbo { RetryQueueId = retryQueueId },
            new RetryQueueItemDbo { RetryQueueId = retryQueueId },
            new RetryQueueItemDbo { RetryQueueId = retryQueueId },
        };

        // Act
        Action act = () => adapter.Adapt(queuesDbo, itemsDbo);

        // Assert
        act.Should()
            .Throw<RetryDurableException>()
            .WithMessage("RetryQueueId:a278590f-299b-4f4c-88f0-1ea3c4588786 RetryQueueItemId:4df0634d-3207-485d-9c5e-100bff4607c5 RetryQueueItemSort:1");
    }
}