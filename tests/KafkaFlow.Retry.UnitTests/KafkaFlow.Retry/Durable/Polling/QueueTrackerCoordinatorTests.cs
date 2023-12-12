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
    private readonly Mock<IJobDataProvider> _mockJobDataProvider;
    private readonly Mock<IQueueTrackerFactory> _mockQueueTrackerFactory;
    private readonly IQueueTrackerCoordinator _queueTrackerCoordinator;

    public QueueTrackerCoordinatorTests()
    {
            var pollingDefinition = new RetryDurablePollingDefinition(true, "0 0/1 * 1/1 * ? *", 1, 1);

            _mockJobDataProvider = new Mock<IJobDataProvider>();

            _mockJobDataProvider
                .SetupGet(m => m.PollingDefinition)
                .Returns(pollingDefinition);

            var mockTrigger = new Mock<ITrigger>();
            mockTrigger
                .SetupGet(m => m.Key)
                .Returns(new TriggerKey("someTriggerKey"));

            _mockJobDataProvider
                .SetupGet(m => m.Trigger)
                .Returns(mockTrigger.Object);

            _mockQueueTrackerFactory = new Mock<IQueueTrackerFactory>();
            _mockQueueTrackerFactory
                .Setup(d => d.Create(It.IsAny<IMessageProducer>(), It.IsAny<ILogHandler>()))
                .Returns(new QueueTracker(
                    "id",
                    new[] { _mockJobDataProvider.Object },
                    Mock.Of<ILogHandler>()));

            _queueTrackerCoordinator = new QueueTrackerCoordinator(_mockQueueTrackerFactory.Object);
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
            await _queueTrackerCoordinator.ScheduleJobsAsync(Mock.Of<IMessageProducer>(), Mock.Of<ILogHandler>());

            //Assert
            _mockQueueTrackerFactory.Verify(d => d.Create(It.IsAny<IMessageProducer>(), It.IsAny<ILogHandler>()), Times.Once);
            _mockJobDataProvider.Verify(m => m.JobDetail, Times.Once);
            _mockJobDataProvider.Verify(m => m.Trigger, Times.Once);
        }

    [Fact]
    public async Task QueueTrackerCoordinator_UnscheduleJobs_Success()
    {
            // Arrange

            _mockJobDataProvider
                .Setup(x => x.JobDetail)
                .Returns(
                    JobBuilder
                        .Create<RetryDurablePollingJob>()
                        .Build());

            // Act
            await _queueTrackerCoordinator.ScheduleJobsAsync(Mock.Of<IMessageProducer>(), Mock.Of<ILogHandler>());
            await _queueTrackerCoordinator.UnscheduleJobsAsync();

            //Assert
            _mockQueueTrackerFactory.Verify(d => d.Create(It.IsAny<IMessageProducer>(), It.IsAny<ILogHandler>()), Times.Once);
            _mockJobDataProvider.Verify(m => m.JobDetail, Times.Once);
            _mockJobDataProvider.Verify(m => m.Trigger, Times.Exactly(2));
        }
}