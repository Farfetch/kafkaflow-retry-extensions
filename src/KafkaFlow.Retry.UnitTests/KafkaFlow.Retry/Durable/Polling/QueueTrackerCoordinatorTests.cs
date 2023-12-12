using System;
using System.Threading.Tasks;
using KafkaFlow.Retry.Durable.Definitions.Polling;
using KafkaFlow.Retry.Durable.Polling;
using KafkaFlow.Retry.Durable.Polling.Jobs;
using Moq;
using Quartz;

namespace KafkaFlow.Retry.UnitTests.KafkaFlow.Retry.Durable.Polling;

public class QueueTrackerCoordinatorTests
{
    private readonly Mock<IJobDataProvider> mockJobDataProvider;
    private readonly Mock<IQueueTrackerFactory> mockQueueTrackerFactory;
    private readonly IQueueTrackerCoordinator queueTrackerCoordinator;

    public QueueTrackerCoordinatorTests()
    {
            var pollingDefinition = new RetryDurablePollingDefinition(true, "0 0/1 * 1/1 * ? *", 1, 1);

            mockJobDataProvider = new Mock<IJobDataProvider>();

            mockJobDataProvider
                .SetupGet(m => m.PollingDefinition)
                .Returns(pollingDefinition);

            var mockTrigger = new Mock<ITrigger>();
            mockTrigger
                .SetupGet(m => m.Key)
                .Returns(new TriggerKey("someTriggerKey"));

            mockJobDataProvider
                .SetupGet(m => m.Trigger)
                .Returns(mockTrigger.Object);

            mockQueueTrackerFactory = new Mock<IQueueTrackerFactory>();
            mockQueueTrackerFactory
                .Setup(d => d.Create(It.IsAny<IMessageProducer>(), It.IsAny<ILogHandler>()))
                .Returns(new QueueTracker(
                    "id",
                    new[] { mockJobDataProvider.Object },
                    Mock.Of<ILogHandler>()));

            queueTrackerCoordinator = new QueueTrackerCoordinator(mockQueueTrackerFactory.Object);
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
            await queueTrackerCoordinator.ScheduleJobsAsync(Mock.Of<IMessageProducer>(), Mock.Of<ILogHandler>());

            //Assert
            mockQueueTrackerFactory.Verify(d => d.Create(It.IsAny<IMessageProducer>(), It.IsAny<ILogHandler>()), Times.Once);
            mockJobDataProvider.Verify(m => m.JobDetail, Times.Once);
            mockJobDataProvider.Verify(m => m.Trigger, Times.Once);
        }

    [Fact]
    public async Task QueueTrackerCoordinator_UnscheduleJobs_Success()
    {
            // Arrange

            mockJobDataProvider
                .Setup(x => x.JobDetail)
                .Returns(
                    JobBuilder
                        .Create<RetryDurablePollingJob>()
                        .Build());

            // Act
            await queueTrackerCoordinator.ScheduleJobsAsync(Mock.Of<IMessageProducer>(), Mock.Of<ILogHandler>());
            await queueTrackerCoordinator.UnscheduleJobsAsync();

            //Assert
            mockQueueTrackerFactory.Verify(d => d.Create(It.IsAny<IMessageProducer>(), It.IsAny<ILogHandler>()), Times.Once);
            mockJobDataProvider.Verify(m => m.JobDetail, Times.Once);
            mockJobDataProvider.Verify(m => m.Trigger, Times.Exactly(2));
        }
}