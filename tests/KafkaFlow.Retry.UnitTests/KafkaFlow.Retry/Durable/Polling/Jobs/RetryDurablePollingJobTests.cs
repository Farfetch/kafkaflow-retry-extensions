using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KafkaFlow.Retry.Durable;
using KafkaFlow.Retry.Durable.Common;
using KafkaFlow.Retry.Durable.Definitions.Polling;
using KafkaFlow.Retry.Durable.Encoders;
using KafkaFlow.Retry.Durable.Polling.Jobs;
using KafkaFlow.Retry.Durable.Repository;
using KafkaFlow.Retry.Durable.Repository.Actions.Read;
using KafkaFlow.Retry.Durable.Repository.Actions.Update;
using KafkaFlow.Retry.Durable.Repository.Adapters;
using KafkaFlow.Retry.Durable.Repository.Model;
using Moq;
using Quartz;

namespace KafkaFlow.Retry.UnitTests.KafkaFlow.Retry.Durable.Polling.Jobs;

public class RetryDurablePollingJobTests
{
    private const string SchedulerId = "schedulerIdTest";
    private static readonly RetryDurablePollingDefinition s_retryDurablePollingDefinition = new RetryDurablePollingDefinition(true, "0 0 14-6 ? * FRI-MON", 1, 1);
    private readonly IJob _job = new RetryDurablePollingJob();
    private readonly Mock<IJobExecutionContext> _jobExecutionContext = new Mock<IJobExecutionContext>();
    private readonly Mock<ILogHandler> _logHandler = new Mock<ILogHandler>();
    private readonly Mock<IMessageAdapter> _messageAdapter = new Mock<IMessageAdapter>();
    private readonly Mock<IMessageHeadersAdapter> _messageHeadersAdapter = new Mock<IMessageHeadersAdapter>();
    private readonly Mock<IMessageProducer> _messageProducer = new Mock<IMessageProducer>();
    private readonly Mock<IJobDetail> _mockIJobDetail = new Mock<IJobDetail>();
    private readonly Mock<ITrigger> _mockITrigger = new Mock<ITrigger>();
    private readonly Mock<IRetryDurableQueueRepository> _retryDurableQueueRepository = new Mock<IRetryDurableQueueRepository>();
    private readonly Mock<IUtf8Encoder> _utf8Encoder = new Mock<IUtf8Encoder>();

    public RetryDurablePollingJobTests()
    {
        _jobExecutionContext
            .Setup(d => d.JobDetail)
            .Returns(_mockIJobDetail.Object);

        _mockITrigger
            .SetupGet(t => t.Key)
            .Returns(new TriggerKey(string.Empty));

        _jobExecutionContext
            .Setup(d => d.Trigger)
            .Returns(_mockITrigger.Object);
    }

    [Fact]
    public async Task RetryDurablePollingJob_Execute_ProduceMessageFailed_LogError()
    {
        // Arrange
        _retryDurableQueueRepository
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
        _retryDurableQueueRepository
            .Setup(d => d.UpdateItemAsync(It.IsAny<UpdateItemStatusInput>()));

        _messageHeadersAdapter
            .Setup(d => d.AdaptMessageHeadersFromRepository(It.IsAny<IList<MessageHeader>>()))
            .Returns(new MessageHeaders());

        _messageProducer
            .Setup(d => d.ProduceAsync(It.IsAny<byte[]>(), It.IsAny<byte[]>(), It.IsAny<IMessageHeaders>(), It.IsAny<int?>()))
            .Throws(new Exception());

        IDictionary<string, object> data = new Dictionary<string, object>
        {
            { "RetryDurableQueueRepository", _retryDurableQueueRepository.Object },
            { "RetryDurableProducer", _messageProducer.Object },
            { "RetryDurablePollingDefinition", s_retryDurablePollingDefinition},
            { "LogHandler", _logHandler.Object },
            { "MessageHeadersAdapter", _messageHeadersAdapter.Object },
            { "MessageAdapter", _messageAdapter.Object },
            { "Utf8Encoder", _utf8Encoder.Object },
            { "SchedulerId", SchedulerId }
        };

        _mockIJobDetail
            .SetupGet(jd => jd.JobDataMap)
            .Returns(new JobDataMap(data));

        // Act
        await _job.Execute(_jobExecutionContext.Object);

        //Assert
        _logHandler.Verify(d => d.Error(It.IsAny<string>(), It.IsAny<Exception>(), It.IsAny<object>()), Times.Exactly(2));
        _retryDurableQueueRepository.Verify(d => d.GetRetryQueuesAsync(It.IsAny<GetQueuesInput>()), Times.Once);
        _retryDurableQueueRepository.Verify(d => d.UpdateItemAsync(It.IsAny<UpdateItemStatusInput>()), Times.Exactly(2));
        _messageHeadersAdapter.Verify(d => d.AdaptMessageHeadersFromRepository(It.IsAny<IList<MessageHeader>>()), Times.Once);

        _retryDurableQueueRepository.Reset();
        _messageProducer.Reset();
    }

    [Fact]
    public async Task RetryDurablePollingJob_Execute_RetryDurableQueueRepositoryFailed_LogError()
    {
        // Arrange
        _retryDurableQueueRepository
            .Setup(d => d.GetRetryQueuesAsync(It.IsAny<GetQueuesInput>()))
            .Throws(new RetryDurableException(new RetryError(RetryErrorCode.ConsumerBlockedException), "error"));

        IDictionary<string, object> data = new Dictionary<string, object>
        {
            { "RetryDurableQueueRepository", _retryDurableQueueRepository.Object },
            { "RetryDurableProducer", _messageProducer.Object },
            { "RetryDurablePollingDefinition", s_retryDurablePollingDefinition},
            { "LogHandler", _logHandler.Object },
            { "MessageHeadersAdapter", _messageHeadersAdapter.Object },
            { "MessageAdapter", _messageAdapter.Object },
            { "Utf8Encoder", _utf8Encoder.Object },
            { "SchedulerId", SchedulerId }
        };

        _mockIJobDetail
            .SetupGet(jd => jd.JobDataMap)
            .Returns(new JobDataMap(data));

        // Act
        await _job.Execute(_jobExecutionContext.Object);

        //Assert
        _messageProducer.Verify(d => d.ProduceAsync(It.IsAny<byte[]>(), It.IsAny<byte[]>(), It.IsAny<IMessageHeaders>(), It.IsAny<int?>()), Times.Never);
        _logHandler.Verify(d => d.Error(It.IsAny<string>(), It.IsAny<RetryDurableException>(), It.IsAny<object>()), Times.Once);
        _retryDurableQueueRepository.Verify(d => d.GetRetryQueuesAsync(It.IsAny<GetQueuesInput>()), Times.Once);
        _retryDurableQueueRepository.Verify(d => d.UpdateItemAsync(It.IsAny<UpdateItemStatusInput>()), Times.Never);
        _messageHeadersAdapter.Verify(d => d.AdaptMessageHeadersFromRepository(It.IsAny<IList<MessageHeader>>()), Times.Never);
    }

    [Fact]
    public async Task RetryDurablePollingJob_Execute_Success()
    {
        // Arrange
        _retryDurableQueueRepository
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
        _retryDurableQueueRepository
            .Setup(d => d.UpdateItemAsync(It.IsAny<UpdateItemStatusInput>()));

        _messageHeadersAdapter
            .Setup(d => d.AdaptMessageHeadersFromRepository(It.IsAny<IList<MessageHeader>>()))
            .Returns(new MessageHeaders());

        _messageProducer
            .Setup(d => d.ProduceAsync(It.IsAny<byte[]>(), It.IsAny<byte[]>(), It.IsAny<IMessageHeaders>(), It.IsAny<int?>()));

        IDictionary<string, object> data = new Dictionary<string, object>
        {
            { "RetryDurableQueueRepository", _retryDurableQueueRepository.Object },
            { "RetryDurableProducer", _messageProducer.Object },
            { "RetryDurablePollingDefinition", s_retryDurablePollingDefinition},
            { "LogHandler", _logHandler.Object },
            { "MessageHeadersAdapter", _messageHeadersAdapter.Object },
            { "MessageAdapter", _messageAdapter.Object },
            { "Utf8Encoder", _utf8Encoder.Object },
            { "SchedulerId", SchedulerId }
        };
        _mockIJobDetail
            .SetupGet(jd => jd.JobDataMap)
            .Returns(new JobDataMap(data));

        // Act
        await _job.Execute(_jobExecutionContext.Object);

        //Assert
        _messageProducer.Verify(d => d.ProduceAsync(It.IsAny<byte[]>(), It.IsAny<byte[]>(), It.IsAny<IMessageHeaders>(), It.IsAny<int?>()), Times.Once);
        _retryDurableQueueRepository.Verify(d => d.GetRetryQueuesAsync(It.IsAny<GetQueuesInput>()), Times.Once);
        _retryDurableQueueRepository.Verify(d => d.UpdateItemAsync(It.IsAny<UpdateItemStatusInput>()), Times.Once);
        _messageHeadersAdapter.Verify(d => d.AdaptMessageHeadersFromRepository(It.IsAny<IList<MessageHeader>>()), Times.Once);
    }
}