using System;
using System.Threading.Tasks;
using FluentAssertions;
using global::KafkaFlow.Retry.Durable.Definitions.Polling;
using global::KafkaFlow.Retry.Durable.Polling;
using global::KafkaFlow.Retry.Durable.Polling.Jobs;
using Moq;
using Quartz;
using Xunit;

namespace KafkaFlow.Retry.UnitTests.KafkaFlow.Retry.Durable.Polling;

public class QueueTrackerCoordinatorTests
{
    private readonly Mock<IJobDataProvider> mockJobDataProvider;
    private readonly Mock<IQueueTrackerFactory> mockQueueTrackerFactory;
    private readonly IQueueTrackerCoordinator queueTrackerCoordinator;

    public QueueTrackerCoordinatorTests()
    {
            var pollingDefinition = new RetryDurablePollingDefinition(true, "0 0/1 * 1/1 * ? *", 1, 1);

            this.mockJobDataProvider = new Mock<IJobDataProvider>();

            this.mockJobDataProvider
                .SetupGet(m => m.PollingDefinition)
                .Returns(pollingDefinition);

            var mockTrigger = new Mock<ITrigger>();
            mockTrigger
                .SetupGet(m => m.Key)
                .Returns(new TriggerKey("someTriggerKey"));

            this.mockJobDataProvider
                .SetupGet(m => m.Trigger)
                .Returns(mockTrigger.Object);

            this.mockQueueTrackerFactory = new Mock<IQueueTrackerFactory>();
            mockQueueTrackerFactory
                .Setup(d => d.Create(It.IsAny<IMessageProducer>(), It.IsAny<ILogHandler>()))
                .Returns(new QueueTracker(
                    "id",
                    new[] { this.mockJobDataProvider.Object },
                    Mock.Of<ILogHandler>()));

            this.queueTrackerCoordinator = new QueueTrackerCoordinator(mockQueueTrackerFactory.Object);
        }

    [Fact]
    public void QueueTrackerCoordinator_Ctor_WithArgumentNull_ThrowsException()
    {
            // Arrange & Act
            Action act = () => new QueueTrackerCoordinator(null);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

    [Fact]
    public async Task QueueTrackerCoordinator_ScheduleJobs_Success()
    {
            // Act
            await this.queueTrackerCoordinator.ScheduleJobsAsync(Mock.Of<IMessageProducer>(), Mock.Of<ILogHandler>());

            //Assert
            this.mockQueueTrackerFactory.Verify(d => d.Create(It.IsAny<IMessageProducer>(), It.IsAny<ILogHandler>()), Times.Once);
            this.mockJobDataProvider.Verify(m => m.JobDetail, Times.Once);
            this.mockJobDataProvider.Verify(m => m.Trigger, Times.Once);
        }

    [Fact]
    public async Task QueueTrackerCoordinator_UnscheduleJobs_Success()
    {
            // Arrange

            this.mockJobDataProvider
                .Setup(x => x.JobDetail)
                .Returns(
                    JobBuilder
                        .Create<RetryDurablePollingJob>()
                        .Build());

            // Act
            await this.queueTrackerCoordinator.ScheduleJobsAsync(Mock.Of<IMessageProducer>(), Mock.Of<ILogHandler>());
            await this.queueTrackerCoordinator.UnscheduleJobsAsync();

            //Assert
            this.mockQueueTrackerFactory.Verify(d => d.Create(It.IsAny<IMessageProducer>(), It.IsAny<ILogHandler>()), Times.Once);
            this.mockJobDataProvider.Verify(m => m.JobDetail, Times.Once);
            this.mockJobDataProvider.Verify(m => m.Trigger, Times.Exactly(2));
        }
}