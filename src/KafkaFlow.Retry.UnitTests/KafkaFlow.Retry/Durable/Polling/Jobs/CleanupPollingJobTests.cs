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
    private static readonly CleanupPollingDefinition cleaunupPollingDefinition = new CleanupPollingDefinition(true, "0 0 14-6 ? * FRI-MON", 1, 10);
    private readonly IJob job = new CleanupPollingJob();
    private readonly Mock<IJobDetail> mockIJobDetail = new Mock<IJobDetail>();
    private readonly Mock<ITrigger> mockITrigger = new Mock<ITrigger>();
    private readonly Mock<IJobExecutionContext> mockJobExecutionContext = new Mock<IJobExecutionContext>();
    private readonly Mock<ILogHandler> mockLogHandler = new Mock<ILogHandler>();
    private readonly Mock<IRetryDurableQueueRepository> mockRetryDurableQueueRepository = new Mock<IRetryDurableQueueRepository>();

    public CleanupPollingJobTests()
    {
        mockJobExecutionContext
            .Setup(d => d.JobDetail)
            .Returns(mockIJobDetail.Object);

        mockITrigger
            .SetupGet(t => t.Key)
            .Returns(new TriggerKey(string.Empty));

        mockJobExecutionContext
            .Setup(d => d.Trigger)
            .Returns(mockITrigger.Object);

        IDictionary<string, object> jobData = new Dictionary<string, object>
        {
            { "RetryDurableQueueRepository", mockRetryDurableQueueRepository.Object },
            { "CleanupPollingDefinition", cleaunupPollingDefinition},
            { "LogHandler", mockLogHandler.Object },
            { "SchedulerId", SchedulerId }
        };

        mockIJobDetail
            .SetupGet(jd => jd.JobDataMap)
            .Returns(new JobDataMap(jobData));
    }

    [Fact]
    public async Task CleanupPollingJob_Execute_RetryDurableQueueRepositoryFailed_LogError()
    {
        // Arrange
        mockRetryDurableQueueRepository
            .Setup(d => d.DeleteQueuesAsync(It.IsAny<DeleteQueuesInput>()))
            .Throws(new RetryDurableException(new RetryError(RetryErrorCode.Consumer_BlockedException), "error"));

        // Act
        await job.Execute(mockJobExecutionContext.Object);

        //Assert
        mockLogHandler.Verify(d => d.Info(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        mockLogHandler.Verify(d => d.Error(It.IsAny<string>(), It.IsAny<RetryDurableException>(), It.IsAny<object>()), Times.Once);
        mockRetryDurableQueueRepository.Verify(d => d.DeleteQueuesAsync(It.IsAny<DeleteQueuesInput>()), Times.Once);
    }

    [Fact]
    public async Task CleanupPollingJob_Execute_Success()
    {
        // Arrange
        mockRetryDurableQueueRepository
            .Setup(d => d.DeleteQueuesAsync(It.IsAny<DeleteQueuesInput>()))
            .ReturnsAsync(new DeleteQueuesResult(1));

        // Act
        await job.Execute(mockJobExecutionContext.Object);

        //Assert
        mockLogHandler.Verify(d => d.Info(It.IsAny<string>(), It.IsAny<object>()), Times.Exactly(2));
        mockLogHandler.Verify(d => d.Error(It.IsAny<string>(), It.IsAny<RetryDurableException>(), It.IsAny<object>()), Times.Never);
        mockRetryDurableQueueRepository.Verify(d => d.DeleteQueuesAsync(It.IsAny<DeleteQueuesInput>()), Times.Once);
    }
}