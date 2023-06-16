namespace KafkaFlow.Retry.UnitTests.KafkaFlow.Retry.Durable.Polling
{
    using System;
    using FluentAssertions;
    using global::KafkaFlow.Retry.Durable.Definitions;
    using global::KafkaFlow.Retry.Durable.Polling;
    using Moq;
    using Quartz;
    using Xunit;

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
        public void QueueTrackerCoordinator_ScheduleJob_Success()
        {
            // Arrange
            queueTrackerFactory
                .Setup(d => d.Create(It.IsAny<RetryDurablePollingDefinition>(), It.IsAny<IMessageProducer>(), It.IsAny<ILogHandler>()))
                .Returns(new QueueTracker(
                    Mock.Of<ILogHandler>(),
                    retryDurablePollingDefinition,
                    Mock.Of<IJobDetailProvider>(),
                    Mock.Of<ITriggerProvider>()
                    ));

            var coordinator = new QueueTrackerCoordinator(queueTrackerFactory.Object);

            // Act
            coordinator.ScheduleJob(retryDurablePollingDefinition,
                                mockIMessageProducer.Object,
                                mockILogHandler.Object);

            //Assert
            queueTrackerFactory.Verify(d => d.Create(It.IsAny<RetryDurablePollingDefinition>(), It.IsAny<IMessageProducer>(), It.IsAny<ILogHandler>()), Times.Once);
        }

        [Fact]
        public void QueueTrackerCoordinator_UnscheduleJob_Success()
        {
            // Arrange
            var mockIJobDetailProvider = new Mock<IJobDetailProvider>();
            mockIJobDetailProvider
                .Setup(x => x.GetQueuePollingJobDetail())
                .Returns(Mock.Of<IJobDetail>());

            var mockITrigger = new Mock<ITrigger>();
            mockITrigger
                .SetupGet(x => x.Key)
                .Returns(new TriggerKey(String.Empty));

            var mockITriggerProvider = new Mock<ITriggerProvider>();
            mockITriggerProvider
                .Setup(x => x.GetQueuePollingTrigger())
                .Returns(mockITrigger.Object);

            queueTrackerFactory
                .Setup(d => d.Create(It.IsAny<RetryDurablePollingDefinition>(), It.IsAny<IMessageProducer>(), It.IsAny<ILogHandler>()))
                .Returns(new QueueTracker(
                    Mock.Of<ILogHandler>(),
                    retryDurablePollingDefinition,
                    mockIJobDetailProvider.Object,
                    mockITriggerProvider.Object
                    ));

            var coordinator = new QueueTrackerCoordinator(queueTrackerFactory.Object);

            // Act
            coordinator.ScheduleJob(
                        retryDurablePollingDefinition,
                        mockIMessageProducer.Object,
                        mockILogHandler.Object);
            coordinator.UnscheduleJob();

            //Assert
            queueTrackerFactory.Verify(d => d.Create(It.IsAny<RetryDurablePollingDefinition>(), It.IsAny<IMessageProducer>(), It.IsAny<ILogHandler>()), Times.Once);
        }
    }
}