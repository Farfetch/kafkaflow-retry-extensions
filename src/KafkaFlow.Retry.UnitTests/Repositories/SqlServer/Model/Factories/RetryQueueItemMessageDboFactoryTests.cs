using System;

namespace KafkaFlow.Retry.UnitTests.Repositories.SqlServer.Model.Factories
{
    using System.Diagnostics.CodeAnalysis;
    using FluentAssertions;
    using global::KafkaFlow.Retry.Durable.Repository.Model;
    using global::KafkaFlow.Retry.SqlServer.Model;
    using global::KafkaFlow.Retry.SqlServer.Model.Factories;
    using Xunit;

    [ExcludeFromCodeCoverage]
    public class RetryQueueItemMessageDboFactoryTests
    {
        private readonly RetryQueueItemMessageDboFactory factory = new RetryQueueItemMessageDboFactory();
        private readonly RetryQueueItemMessage message = new RetryQueueItemMessage("topicName", new byte[] { 1, 3 }, new byte[] { 2, 4, 6 }, 3, 21, DateTime.UtcNow);

        [Fact]
        public void RetryQueueItemMessageDboFactory_Create_Success()
        {
            // Act
            var result = factory.Create(message, 1);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType(typeof(RetryQueueItemMessageDbo));
        }

        [Fact]
        public void RetryQueueItemMessageDboFactory_Create_WithNegativeRetryQueueItemId_ThrowsException()
        {
            // Act
            Action act = () => factory.Create(message, -1);

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void RetryQueueItemMessageDboFactory_Create_WithoutSaveToQueueInput_ThrowsException()
        {
            //Arrange
            RetryQueueItemMessage messasgeNull = null;

            // Act
            Action act = () => factory.Create(messasgeNull, 1);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }
    }
}