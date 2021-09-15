namespace KafkaFlow.Retry.UnitTests.KafkaFlow.Retry.Durable.Polling
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using global::KafkaFlow.Retry.Durable.Definitions;
    using global::KafkaFlow.Retry.Durable.Encoders;
    using global::KafkaFlow.Retry.Durable.Polling;
    using global::KafkaFlow.Retry.Durable.Repository;
    using global::KafkaFlow.Retry.Durable.Repository.Adapters;
    using Moq;
    using Xunit;

    public class QueueTrackerCoordinatorTests
    {
        public static IEnumerable<object[]> DataTest = new List<object[]>
        {
            new object[]
            {
                null,
                Mock.Of<IRetryDurablePollingDefinition>()
            },
            new object[]
            {
                Mock.Of<IQueueTrackerFactory>(),
                null
            }
        };

        private readonly Mock<IQueueTrackerFactory> queueTrackerFactory = new Mock<IQueueTrackerFactory>();
        private readonly Mock<IRetryDurablePollingDefinition> retryDurablePollingDefinition = new Mock<IRetryDurablePollingDefinition>();

        [Theory]
        [MemberData(nameof(DataTest))]
        public void QueueTrackerCoordinator_Ctor_WithArgumentNull_ThrowsException(
            object queueTrackerFactory,
            object retryDurablePollingDefinition)
        {
            // Arrange & Act
            Action act = () => new QueueTrackerCoordinator((IQueueTrackerFactory)queueTrackerFactory,
                (IRetryDurablePollingDefinition)retryDurablePollingDefinition);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void QueueTrackerCoordinator_Initialize_Success()
        {
            // Arrange
            retryDurablePollingDefinition
                .Setup(d => d.Enabled)
                .Returns(true);

            retryDurablePollingDefinition
                .Setup(d => d.CronExpression)
                .Returns("0 0 14-6 ? * FRI-MON");

            queueTrackerFactory
                .Setup(d => d.Create())
                .Returns(new QueueTracker(
                    Mock.Of<IRetryDurableQueueRepository>(),
                    Mock.Of<ILogHandler>(),
                    Mock.Of<IMessageHeadersAdapter>(),
                    Mock.Of<IMessageAdapter>(),
                    Mock.Of<IUtf8Encoder>(),
                    Mock.Of<IMessageProducer>(),
                    retryDurablePollingDefinition.Object
                    ));

            var coordinator = new QueueTrackerCoordinator(queueTrackerFactory.Object,
                retryDurablePollingDefinition.Object);

            // Act
            coordinator.Initialize();

            //Assert
            queueTrackerFactory.Verify(d => d.Create(), Times.Once);
        }

        [Fact]
        public void QueueTrackerCoordinator_Shutdown_Success()
        {
            // Arrange
            retryDurablePollingDefinition
                .Setup(d => d.Enabled)
                .Returns(true);

            retryDurablePollingDefinition
                .Setup(d => d.CronExpression)
                .Returns("0 0 14-6 ? * FRI-MON");

            queueTrackerFactory
                .Setup(d => d.Create())
                .Returns(new QueueTracker(
                    Mock.Of<IRetryDurableQueueRepository>(),
                    Mock.Of<ILogHandler>(),
                    Mock.Of<IMessageHeadersAdapter>(),
                    Mock.Of<IMessageAdapter>(),
                    Mock.Of<IUtf8Encoder>(),
                    Mock.Of<IMessageProducer>(),
                    retryDurablePollingDefinition.Object
                    ));

            var coordinator = new QueueTrackerCoordinator(queueTrackerFactory.Object,
                retryDurablePollingDefinition.Object);

            // Act
            coordinator.Initialize();
            coordinator.Shutdown();

            //Assert
            queueTrackerFactory.Verify(d => d.Create(), Times.Once);
        }
    }
}