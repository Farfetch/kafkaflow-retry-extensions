using System;
using KafkaFlow.Retry.Durable.Common;
using KafkaFlow.Retry.Durable.Repository.Actions.Create;
using KafkaFlow.Retry.Durable.Repository.Model;
using KafkaFlow.Retry.MongoDb.Adapters.Interfaces;
using KafkaFlow.Retry.MongoDb.Model;
using KafkaFlow.Retry.MongoDb.Model.Factories;
using Moq;

namespace KafkaFlow.Retry.UnitTests.Repositories.MongoDb.Model.Factories;

public class RetryQueueItemDboFactoryTests
{
    private readonly RetryQueueItemDboFactory factory;
    private readonly Mock<IMessageAdapter> messageAdapter = new Mock<IMessageAdapter>();

    private readonly SaveToQueueInput saveToQueueInput = new SaveToQueueInput(
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

    public RetryQueueItemDboFactoryTests()
    {
            var retryQueueItemMessage = new RetryQueueItemMessage("topicName", new byte[] { 1, 3 }, new byte[] { 2, 4, 6 }, 3, 21, DateTime.UtcNow);
            messageAdapter.Setup(d => d.Adapt(It.IsAny<RetryQueueItemMessageDbo>())).Returns(retryQueueItemMessage);
            factory = new RetryQueueItemDboFactory(messageAdapter.Object);
        }

    [Fact]
    public void RetryQueueItemDboFactory_Create_Success()
    {
            // Act
            var result = factory.Create(saveToQueueInput, Guid.NewGuid());

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType(typeof(RetryQueueItemDbo));
        }

    [Fact]
    public void RetryQueueItemDboFactory_Create_WithDefaultQueueId_ThrowsException()
    {
            // Act
            Action act = () => factory.Create(saveToQueueInput, default);

            // Assert
            act.Should().Throw<ArgumentException>();
        }

    [Fact]
    public void RetryQueueItemDboFactory_Create_WithNegativeSort_ThrowsException()
    {
            // Act
            Action act = () => factory.Create(saveToQueueInput, Guid.NewGuid(), -1);

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

    [Fact]
    public void RetryQueueItemDboFactory_Create_WithoutSaveToQueueInput_ThrowsException()
    {
            //Arrange
            SaveToQueueInput saveToQueueInputNull = null;

            // Act
            Action act = () => factory.Create(saveToQueueInputNull, Guid.NewGuid());

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }
}