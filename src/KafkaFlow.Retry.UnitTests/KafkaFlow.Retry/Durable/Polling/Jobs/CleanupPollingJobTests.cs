using System.Collections.Generic;
using System.Threading.Tasks;
using KafkaFlow.Retry.Durable;
using KafkaFlow.Retry.Durable.Definitions.Polling;
using KafkaFlow.Retry.Durable.Polling.Jobs;
using KafkaFlow.Retry.Durable.Repository;
using KafkaFlow.Retry.Durable.Repository.Actions.Delete;
using Moq;
using Quartz;

namespace KafkaFlow.Retry.UnitTests.KafkaFlow.Retry.Durable.Polling.Jobs;

public class CleanupPollingJobTests
{
    private const string SchedulerId = "schedulerIdTest";
    private static readonly CleanupPollingDefinition s_cleaunupPollingDefinition = new CleanupPollingDefinition(true, "0 0 14-6 ? * FRI-MON", 1, 10);
    private readonly IJob _job = new CleanupPollingJob();
    private readonly Mock<IJobDetail> _mockIJobDetail = new Mock<IJobDetail>();
    private readonly Mock<ITrigger> _mockITrigger = new Mock<ITrigger>();
    private readonly Mock<IJobExecutionContext> _mockJobExecutionContext = new Mock<IJobExecutionContext>();
    private readonly Mock<ILogHandler> _mockLogHandler = new Mock<ILogHandler>();
    private readonly Mock<IRetryDurableQueueRepository> _mockRetryDurableQueueRepository = new Mock<IRetryDurableQueueRepository>();

    public CleanupPollingJobTests()
    {
        _mockJobExecutionContext
            .Setup(d => d.JobDetail)
            .Returns(_mockIJobDetail.Object);

        _mockITrigger
            .SetupGet(t => t.Key)
            .Returns(new TriggerKey(string.Empty));

        _mockJobExecutionContext
            .Setup(d => d.Trigger)
            .Returns(_mockITrigger.Object);

        IDictionary<string, object> jobData = new Dictionary<string, object>
        {
            { "RetryDurableQueueRepository", _mockRetryDurableQueueRepository.Object },
            { "CleanupPollingDefinition", s_cleaunupPollingDefinition},
            { "LogHandler", _mockLogHandler.Object },
            { "SchedulerId", SchedulerId }
        };

        _mockIJobDetail
            .SetupGet(jd => jd.JobDataMap)
            .Returns(new JobDataMap(jobData));
    }

    [Fact]
    public async Task CleanupPollingJob_Execute_RetryDurableQueueRepositoryFailed_LogError()
    {
        // Arrange
        _mockRetryDurableQueueRepository
            .Setup(d => d.DeleteQueuesAsync(It.IsAny<DeleteQueuesInput>()))
            .Throws(new RetryDurableException(new RetryError(RetryErrorCode.ConsumerBlockedException), "error"));

        // Act
        await _job.Execute(_mockJobExecutionContext.Object);

        //Assert
        _mockLogHandler.Verify(d => d.Info(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        _mockLogHandler.Verify(d => d.Error(It.IsAny<string>(), It.IsAny<RetryDurableException>(), It.IsAny<object>()), Times.Once);
        _mockRetryDurableQueueRepository.Verify(d => d.DeleteQueuesAsync(It.IsAny<DeleteQueuesInput>()), Times.Once);
    }

    [Fact]
    public async Task CleanupPollingJob_Execute_Success()
    {
        // Arrange
        _mockRetryDurableQueueRepository
            .Setup(d => d.DeleteQueuesAsync(It.IsAny<DeleteQueuesInput>()))
            .ReturnsAsync(new DeleteQueuesResult(1));

        // Act
        await _job.Execute(_mockJobExecutionContext.Object);

        //Assert
        _mockLogHandler.Verify(d => d.Info(It.IsAny<string>(), It.IsAny<object>()), Times.Exactly(2));
        _mockLogHandler.Verify(d => d.Error(It.IsAny<string>(), It.IsAny<RetryDurableException>(), It.IsAny<object>()), Times.Never);
        _mockRetryDurableQueueRepository.Verify(d => d.DeleteQueuesAsync(It.IsAny<DeleteQueuesInput>()), Times.Once);
    }
}