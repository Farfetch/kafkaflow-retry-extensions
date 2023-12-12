using System;
using System.Collections.Generic;
using KafkaFlow.Retry.Durable.Common;
using KafkaFlow.Retry.Durable.Repository.Model;
using KafkaFlow.Retry.MongoDb.Model;
using KafkaFlow.Retry.SqlServer.Readers;
using Moq;

namespace KafkaFlow.Retry.UnitTests.Repositories.SqlServer.Readers;

public class DboCollectionNavigatorTests
{
    private readonly DboCollectionNavigator<RetryQueueItemDbo, RetryQueueItem> _dboCollectionNavigator;

    private readonly Mock<IDboDomainAdapter<RetryQueueItemDbo, RetryQueueItem>> _dboDomainAdapter = new Mock<IDboDomainAdapter<RetryQueueItemDbo, RetryQueueItem>>();

    private readonly IList<RetryQueueItemDbo> _dbos = new List<RetryQueueItemDbo>
    {
        new RetryQueueItemDbo
        {
            CreationDate = DateTime.UtcNow,
            Id = Guid.NewGuid(),
            LastExecution = DateTime.UtcNow,
            Description = "description",
            ModifiedStatusDate = DateTime.UtcNow,
            AttemptsCount = 1,
            RetryQueueId = Guid.NewGuid(),
            SeverityLevel = SeverityLevel.High,
            Sort = 1,
            Status = RetryQueueItemStatus.InRetry,
            Message = new RetryQueueItemMessageDbo
            {
                Headers = new List<RetryQueueHeaderDbo>
                {
                    new RetryQueueHeaderDbo()
                },
                Key = new byte[] { 1, 3 },
                Offset = 2,
                Partition = 1,
                TopicName = "topicName",
                UtcTimeStamp = DateTime.UtcNow,
                Value = new byte[] { 2, 4, 6 }
            }
        }
    };

    public DboCollectionNavigatorTests()
    {
        _dboCollectionNavigator = new DboCollectionNavigator<RetryQueueItemDbo, RetryQueueItem>(_dbos, _dboDomainAdapter.Object);
    }

    public static IEnumerable<object[]> DataTestCtor() => new List<object[]>
    {
        new object[]
        {
            null,
            Mock.Of<IDboDomainAdapter<RetryQueueItemDbo, RetryQueueItem>>()
        },
        new object[]
        {
            Mock.Of<IList<RetryQueueItemDbo>>(),
            null
        }
    };

    public static IEnumerable<object[]> DataTestNavigate() => new List<object[]>
    {
        new object[]
        {
            null,
            new Predicate<RetryQueueItemDbo>((_)=> true)
        },
        new object[]
        {
            new Action<RetryQueueItem>((_) =>
                new RetryQueueItem(Guid.NewGuid(), 1, DateTime.UtcNow,0,null,null, RetryQueueItemStatus.Waiting, SeverityLevel.High, "description")
                {
                    Message = new RetryQueueItemMessage("topicName", new byte[1], new byte[1], 1, 1, DateTime.UtcNow)
                }),
            null
        }
    };

    [Fact]
    public void DboCollectionNavigator_Navigate_Success()
    {
        // Arrange
        var action = new Predicate<RetryQueueItemDbo>((_) => true);
        var navigatingCondition = new Action<RetryQueueItem>((_) =>
            new RetryQueueItem(Guid.NewGuid(), 1, DateTime.UtcNow, 0, null, null, RetryQueueItemStatus.Waiting, SeverityLevel.High, "description")
            {
                Message = new RetryQueueItemMessage("topicName", new byte[1], new byte[1], 1, 1, DateTime.UtcNow)
            });

        // Act
        _dboCollectionNavigator.Navigate(navigatingCondition, action);

        // Assert
        _dboDomainAdapter.Verify(d => d.Adapt(It.IsAny<RetryQueueItemDbo>()), Times.Once);
    }

    [Theory]
    [MemberData(nameof(DataTestNavigate))]
    public void DboCollectionNavigator_Navigate_Validations(
        Action<RetryQueueItem> action,
        Predicate<RetryQueueItemDbo> navigatingCondition)
    {
        // Act
        Action act = () => _dboCollectionNavigator.Navigate(action, navigatingCondition);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [MemberData(nameof(DataTestCtor))]
    internal void DboCollectionNavigator_Ctor_Validations(
        IList<RetryQueueItemDbo> dbos,
        IDboDomainAdapter<RetryQueueItemDbo, RetryQueueItem> dboDomainAdapter)
    {
        // Act
        Action act = () => new DboCollectionNavigator<RetryQueueItemDbo, RetryQueueItem>(dbos, dboDomainAdapter);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}