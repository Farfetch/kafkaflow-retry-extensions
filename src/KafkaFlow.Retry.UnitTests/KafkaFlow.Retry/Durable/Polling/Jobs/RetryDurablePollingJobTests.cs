using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using global::KafkaFlow.Retry.Durable;
using global::KafkaFlow.Retry.Durable.Common;
using global::KafkaFlow.Retry.Durable.Definitions.Polling;
using global::KafkaFlow.Retry.Durable.Encoders;
using global::KafkaFlow.Retry.Durable.Polling.Jobs;
using global::KafkaFlow.Retry.Durable.Repository;
using global::KafkaFlow.Retry.Durable.Repository.Actions.Read;
using global::KafkaFlow.Retry.Durable.Repository.Actions.Update;
using global::KafkaFlow.Retry.Durable.Repository.Adapters;
using global::KafkaFlow.Retry.Durable.Repository.Model;
using Moq;
using Quartz;
using Xunit;

namespace KafkaFlow.Retry.UnitTests.KafkaFlow.Retry.Durable.Polling.Jobs;

public class RetryDurablePollingJobTests
{
    private const string SchedulerId = "schedulerIdTest";
    private static readonly RetryDurablePollingDefinition retryDurablePollingDefinition = new RetryDurablePollingDefinition(true, "0 0 14-6 ? * FRI-MON", 1, 1);
    private readonly IJob job = new RetryDurablePollingJob();
    private readonly Mock<IJobExecutionContext> jobExecutionContext = new Mock<IJobExecutionContext>();
    private readonly Mock<ILogHandler> logHandler = new Mock<ILogHandler>();
    private readonly Mock<IMessageAdapter> messageAdapter = new Mock<IMessageAdapter>();
    private readonly Mock<IMessageHeadersAdapter> messageHeadersAdapter = new Mock<IMessageHeadersAdapter>();
    private readonly Mock<IMessageProducer> messageProducer = new Mock<IMessageProducer>();
    private readonly Mock<IJobDetail> mockIJobDetail = new Mock<IJobDetail>();
    private readonly Mock<ITrigger> mockITrigger = new Mock<ITrigger>();
    private readonly Mock<IRetryDurableQueueRepository> retryDurableQueueRepository = new Mock<IRetryDurableQueueRepository>();
    private readonly Mock<IUtf8Encoder> utf8Encoder = new Mock<IUtf8Encoder>();

    public RetryDurablePollingJobTests()
    {
        jobExecutionContext
            .Setup(d => d.JobDetail)
            .Returns(mockIJobDetail.Object);

        mockITrigger
            .SetupGet(t => t.Key)
            .Returns(new TriggerKey(string.Empty));

        jobExecutionContext
            .Setup(d => d.Trigger)
            .Returns(mockITrigger.Object);
    }

    [Fact]
    public async Task RetryDurablePollingJob_Execute_ProduceMessageFailed_LogError()
    {
        // Arrange
        retryDurableQueueRepository
            .Setup(d => d.GetRetryQueuesAsync(It.IsAny<GetQueuesInput>()))
            .ReturnsAsync(new List<RetryQueue>
            {
                new RetryQueue(
                    Guid.NewGuid(),
                    "search",
                    "queue",
                    DateTime.UtcNow,
                    DateTime.UtcNow,
                    RetryQueueStatus.Active,
                    new List<RetryQueueItem>
                    {
                        new RetryQueueItem(Guid.NewGuid(), 1, DateTime.UtcNow,0,null,null, RetryQueueItemStatus.Waiting, SeverityLevel.High, "description")
                        {
                            Message = new RetryQueueItemMessage("topicName", new byte[1], new byte[1], 1, 1, DateTime.UtcNow)
                        }
                    })
            });
        retryDurableQueueRepository
            .Setup(d => d.UpdateItemAsync(It.IsAny<UpdateItemStatusInput>()));

        messageHeadersAdapter
            .Setup(d => d.AdaptMessageHeadersFromRepository(It.IsAny<IList<MessageHeader>>()))
            .Returns(new MessageHeaders());

        messageProducer
            .Setup(d => d.ProduceAsync(It.IsAny<byte[]>(), It.IsAny<byte[]>(), It.IsAny<IMessageHeaders>(), It.IsAny<int?>()))
            .Throws(new Exception());

        IDictionary<string, object> data = new Dictionary<string, object>
        {
            { "RetryDurableQueueRepository", retryDurableQueueRepository.Object },
            { "RetryDurableProducer", messageProducer.Object },
            { "RetryDurablePollingDefinition", retryDurablePollingDefinition},
            { "LogHandler", logHandler.Object },
            { "MessageHeadersAdapter", messageHeadersAdapter.Object },
            { "MessageAdapter", messageAdapter.Object },
            { "Utf8Encoder", utf8Encoder.Object },
            { "SchedulerId", SchedulerId }
        };

        mockIJobDetail
            .SetupGet(jd => jd.JobDataMap)
            .Returns(new JobDataMap(data));

        // Act
        await job.Execute(jobExecutionContext.Object).ConfigureAwait(false);

        //Assert
        logHandler.Verify(d => d.Error(It.IsAny<string>(), It.IsAny<Exception>(), It.IsAny<object>()), Times.Exactly(2));
        retryDurableQueueRepository.Verify(d => d.GetRetryQueuesAsync(It.IsAny<GetQueuesInput>()), Times.Once);
        retryDurableQueueRepository.Verify(d => d.UpdateItemAsync(It.IsAny<UpdateItemStatusInput>()), Times.Exactly(2));
        messageHeadersAdapter.Verify(d => d.AdaptMessageHeadersFromRepository(It.IsAny<IList<MessageHeader>>()), Times.Once);

        retryDurableQueueRepository.Reset();
        messageProducer.Reset();
    }

    [Fact]
    public async Task RetryDurablePollingJob_Execute_RetryDurableQueueRepositoryFailed_LogError()
    {
        // Arrange
        retryDurableQueueRepository
            .Setup(d => d.GetRetryQueuesAsync(It.IsAny<GetQueuesInput>()))
            .Throws(new RetryDurableException(new RetryError(RetryErrorCode.Consumer_BlockedException), "error"));

        IDictionary<string, object> data = new Dictionary<string, object>
        {
            { "RetryDurableQueueRepository", retryDurableQueueRepository.Object },
            { "RetryDurableProducer", messageProducer.Object },
            { "RetryDurablePollingDefinition", retryDurablePollingDefinition},
            { "LogHandler", logHandler.Object },
            { "MessageHeadersAdapter", messageHeadersAdapter.Object },
            { "MessageAdapter", messageAdapter.Object },
            { "Utf8Encoder", utf8Encoder.Object },
            { "SchedulerId", SchedulerId }
        };

        mockIJobDetail
            .SetupGet(jd => jd.JobDataMap)
            .Returns(new JobDataMap(data));

        // Act
        await job.Execute(jobExecutionContext.Object).ConfigureAwait(false);

        //Assert
        messageProducer.Verify(d => d.ProduceAsync(It.IsAny<byte[]>(), It.IsAny<byte[]>(), It.IsAny<IMessageHeaders>(), It.IsAny<int?>()), Times.Never);
        logHandler.Verify(d => d.Error(It.IsAny<string>(), It.IsAny<RetryDurableException>(), It.IsAny<object>()), Times.Once);
        retryDurableQueueRepository.Verify(d => d.GetRetryQueuesAsync(It.IsAny<GetQueuesInput>()), Times.Once);
        retryDurableQueueRepository.Verify(d => d.UpdateItemAsync(It.IsAny<UpdateItemStatusInput>()), Times.Never);
        messageHeadersAdapter.Verify(d => d.AdaptMessageHeadersFromRepository(It.IsAny<IList<MessageHeader>>()), Times.Never);
    }

    [Fact]
    public async Task RetryDurablePollingJob_Execute_Success()
    {
        // Arrange
        retryDurableQueueRepository
            .Setup(d => d.GetRetryQueuesAsync(It.IsAny<GetQueuesInput>()))
            .ReturnsAsync(new List<RetryQueue>
            {
                new RetryQueue(
                    Guid.NewGuid(),
                    "search",
                    "queue",
                    DateTime.UtcNow,
                    DateTime.UtcNow,
                    RetryQueueStatus.Active,
                    new List<RetryQueueItem>
                    {
                        new RetryQueueItem(Guid.NewGuid(), 1, DateTime.UtcNow,0,null,null, RetryQueueItemStatus.Waiting, SeverityLevel.High, "description")
                        {
                            Message = new RetryQueueItemMessage("topicName", new byte[1], new byte[1], 1, 1, DateTime.UtcNow)
                        }
                    })
            });
        retryDurableQueueRepository
            .Setup(d => d.UpdateItemAsync(It.IsAny<UpdateItemStatusInput>()));

        messageHeadersAdapter
            .Setup(d => d.AdaptMessageHeadersFromRepository(It.IsAny<IList<MessageHeader>>()))
            .Returns(new MessageHeaders());

        messageProducer
            .Setup(d => d.ProduceAsync(It.IsAny<byte[]>(), It.IsAny<byte[]>(), It.IsAny<IMessageHeaders>(), It.IsAny<int?>()));

        IDictionary<string, object> data = new Dictionary<string, object>
        {
            { "RetryDurableQueueRepository", retryDurableQueueRepository.Object },
            { "RetryDurableProducer", messageProducer.Object },
            { "RetryDurablePollingDefinition", retryDurablePollingDefinition},
            { "LogHandler", logHandler.Object },
            { "MessageHeadersAdapter", messageHeadersAdapter.Object },
            { "MessageAdapter", messageAdapter.Object },
            { "Utf8Encoder", utf8Encoder.Object },
            { "SchedulerId", SchedulerId }
        };
        mockIJobDetail
            .SetupGet(jd => jd.JobDataMap)
            .Returns(new JobDataMap(data));

        // Act
        await job.Execute(jobExecutionContext.Object).ConfigureAwait(false);

        //Assert
        messageProducer.Verify(d => d.ProduceAsync(It.IsAny<byte[]>(), It.IsAny<byte[]>(), It.IsAny<IMessageHeaders>(), It.IsAny<int?>()), Times.Once);
        retryDurableQueueRepository.Verify(d => d.GetRetryQueuesAsync(It.IsAny<GetQueuesInput>()), Times.Once);
        retryDurableQueueRepository.Verify(d => d.UpdateItemAsync(It.IsAny<UpdateItemStatusInput>()), Times.Once);
        messageHeadersAdapter.Verify(d => d.AdaptMessageHeadersFromRepository(It.IsAny<IList<MessageHeader>>()), Times.Once);
    }
}