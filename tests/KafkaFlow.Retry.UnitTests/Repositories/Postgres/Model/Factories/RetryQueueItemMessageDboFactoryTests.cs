using System;
using KafkaFlow.Retry.Durable.Repository.Model;
using KafkaFlow.Retry.Postgres.Model;
using KafkaFlow.Retry.Postgres.Model.Factories;

namespace KafkaFlow.Retry.UnitTests.Repositories.Postgres.Model.Factories;

public class RetryQueueItemMessageDboFactoryTests
{
    private readonly RetryQueueItemMessageDboFactory _factory = new RetryQueueItemMessageDboFactory();
    private readonly RetryQueueItemMessage _message = new RetryQueueItemMessage("topicName", new byte[] { 1, 3 }, new byte[] { 2, 4, 6 }, 3, 21, DateTime.UtcNow);

    [Fact]
    public void RetryQueueItemMessageDboFactory_Create_Success()
    {
            // Act
            var result = _factory.Create(_message, 1);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType(typeof(RetryQueueItemMessageDbo));
        }

    [Fact]
    public void RetryQueueItemMessageDboFactory_Create_WithNegativeRetryQueueItemId_ThrowsException()
    {
            // Act
            Action act = () => _factory.Create(_message, -1);

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

    [Fact]
    public void RetryQueueItemMessageDboFactory_Create_WithoutSaveToQueueInput_ThrowsException()
    {
            //Arrange
            RetryQueueItemMessage messasgeNull = null;

            // Act
            Action act = () => _factory.Create(messasgeNull, 1);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }
}