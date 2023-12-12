using System;
using KafkaFlow.Retry.Durable.Common;
using KafkaFlow.Retry.Durable.Repository.Actions.Create;
using KafkaFlow.Retry.Durable.Repository.Model;
using KafkaFlow.Retry.Postgres.Model;
using KafkaFlow.Retry.Postgres.Model.Factories;

namespace KafkaFlow.Retry.UnitTests.Repositories.Postgres.Model.Factories;

public class RetryQueueItemDboFactoryTests
{
    private readonly RetryQueueItemDboFactory _factory = new RetryQueueItemDboFactory();

    private readonly SaveToQueueInput _saveToQueueInput = new SaveToQueueInput(
        new RetryQueueItemMessage("topicName", new byte[] { 1, 3 }, new byte[] { 2, 4, 6 }, 3, 21, DateTime.UtcNow),
        "searchGroupKey",
        "queueGroupKey",
        RetryQueueStatus.Active,
        RetryQueueItemStatus.Done,
        SeverityLevel.High,
        DateTime.UtcNow,
        DateTime.UtcNow,
        DateTime.UtcNow,
        3,
        "description");

    [Fact]
    public void RetryQueueItemDboFactory_Create_Success()
    {
        // Act
        var result = _factory.Create(_saveToQueueInput, 1, Guid.NewGuid());

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType(typeof(RetryQueueItemDbo));
    }

    [Fact]
    public void RetryQueueItemDboFactory_Create_WithDefaultQueueId_ThrowsException()
    {
        // Act
        Action act = () => _factory.Create(_saveToQueueInput, 1, default);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RetryQueueItemDboFactory_Create_WithNegativeRetryQueueId_ThrowsException()
    {
        // Act
        Action act = () => _factory.Create(_saveToQueueInput, -1, default);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void RetryQueueItemDboFactory_Create_WithoutSaveToQueueInput_ThrowsException()
    {
        //Arrange
        SaveToQueueInput saveToQueueInputNull = null;

        // Act
        Action act = () => _factory.Create(saveToQueueInputNull, 1, default);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}