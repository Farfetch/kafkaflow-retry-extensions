namespace KafkaFlow.Retry.UnitTests.KafkaFlow.Retry.Durable.Polling
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using FluentAssertions;
    using global::KafkaFlow.Retry.Durable.Definitions;
    using global::KafkaFlow.Retry.Durable.Encoders;
    using global::KafkaFlow.Retry.Durable.Polling;
    using global::KafkaFlow.Retry.Durable.Repository;
    using global::KafkaFlow.Retry.Durable.Repository.Adapters;
    using Moq;
    using Xunit;

    [ExcludeFromCodeCoverage]
    public class QueueTrackerCoordinatorTests
    {
        private readonly Mock<ILogHandler> mockILogHandler = new Mock<ILogHandler>();
        private readonly Mock<IMessageProducer> mockIMessageProducer = new Mock<IMessageProducer>();
        private readonly Mock<IQueueTrackerFactory> queueTrackerFactory = new Mock<IQueueTrackerFactory>();
        private readonly RetryDurablePollingDefinition retryDurablePollingDefinition = new RetryDurablePollingDefinition(true, "*/30 * * ? * *", 10, 100, "id");

        [Fact]
        public void QueueTrackerCoordinator_Ctor_WithArgumentNull_ThrowsException()
        {
            // Arrange & Act
            Action act = () => new QueueTrackerCoordinator(null);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void QueueTrackerCoordinator_Initialize_Success()
        {
            // Arrange
            queueTrackerFactory
                .Setup(d => d.Create(It.IsAny<RetryDurablePollingDefinition>(), It.IsAny<IMessageProducer>(), It.IsAny<ILogHandler>()))
                .Returns(new QueueTracker(
                    Mock.Of<IRetryDurableQueueRepository>(),
                    Mock.Of<ILogHandler>(),
                    Mock.Of<IMessageHeadersAdapter>(),
                    Mock.Of<IMessageAdapter>(),
                    Mock.Of<IUtf8Encoder>(),
                    Mock.Of<IMessageProducer>(),
                    retryDurablePollingDefinition
                    ));

            var coordinator = new QueueTrackerCoordinator(queueTrackerFactory.Object);

            // Act
            coordinator.Initialize(retryDurablePollingDefinition,
                                mockIMessageProducer.Object,
                                mockILogHandler.Object);

            //Assert
            queueTrackerFactory.Verify(d => d.Create(It.IsAny<RetryDurablePollingDefinition>(), It.IsAny<IMessageProducer>(), It.IsAny<ILogHandler>()), Times.Once);
        }

        [Fact]
        public void QueueTrackerCoordinator_Shutdown_Success()
        {
            // Arrange
            queueTrackerFactory
                .Setup(d => d.Create(It.IsAny<RetryDurablePollingDefinition>(), It.IsAny<IMessageProducer>(), It.IsAny<ILogHandler>()))
                .Returns(new QueueTracker(
                    Mock.Of<IRetryDurableQueueRepository>(),
                    Mock.Of<ILogHandler>(),
                    Mock.Of<IMessageHeadersAdapter>(),
                    Mock.Of<IMessageAdapter>(),
                    Mock.Of<IUtf8Encoder>(),
                    Mock.Of<IMessageProducer>(),
                    retryDurablePollingDefinition
                    ));

            var coordinator = new QueueTrackerCoordinator(queueTrackerFactory.Object);

            // Act
            coordinator.Initialize(
                        retryDurablePollingDefinition,
                        mockIMessageProducer.Object,
                        mockILogHandler.Object);
            coordinator.Shutdown();

            //Assert
            queueTrackerFactory.Verify(d => d.Create(It.IsAny<RetryDurablePollingDefinition>(), It.IsAny<IMessageProducer>(), It.IsAny<ILogHandler>()), Times.Once);
        }
    }
}