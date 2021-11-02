namespace KafkaFlow.Retry.UnitTests.Repositories.SqlServer.Model.Factories
{
    using System;
    using FluentAssertions;
    using global::KafkaFlow.Retry.Durable.Common;
    using global::KafkaFlow.Retry.Durable.Repository.Actions.Create;
    using global::KafkaFlow.Retry.Durable.Repository.Model;
    using global::KafkaFlow.Retry.SqlServer.Model;
    using global::KafkaFlow.Retry.SqlServer.Model.Factories;
    using Xunit;

    public class RetryQueueItemDboFactoryTests
    {
        private readonly RetryQueueItemDboFactory factory = new RetryQueueItemDboFactory();

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

        [Fact]
        public void RetryQueueItemDboFactory_Create_Success()
        {
            // Act
            var result = factory.Create(saveToQueueInput, 1, Guid.NewGuid());

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType(typeof(RetryQueueItemDbo));
        }

        [Fact]
        public void RetryQueueItemDboFactory_Create_WithDefaultQueueId_ThrowsException()
        {
            // Act
            Action act = () => factory.Create(saveToQueueInput, 1, default);

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void RetryQueueItemDboFactory_Create_WithNegativeRetryQueueId_ThrowsException()
        {
            // Act
            Action act = () => factory.Create(saveToQueueInput, -1, default);

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void RetryQueueItemDboFactory_Create_WithNegativeSort_ThrowsException()
        {
            // Act
            Action act = () => factory.Create(saveToQueueInput, 1, default);

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void RetryQueueItemDboFactory_Create_WithoutSaveToQueueInput_ThrowsException()
        {
            //Arrange
            SaveToQueueInput saveToQueueInputNull = null;

            // Act
            Action act = () => factory.Create(saveToQueueInputNull, 1, default);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }
    }
}