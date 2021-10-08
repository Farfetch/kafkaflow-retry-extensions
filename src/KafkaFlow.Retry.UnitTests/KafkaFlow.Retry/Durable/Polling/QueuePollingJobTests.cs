namespace KafkaFlow.Retry.UnitTests.KafkaFlow.Retry.Durable.Polling
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using FluentAssertions;
    using global::KafkaFlow.Retry.Durable;
    using global::KafkaFlow.Retry.Durable.Common;
    using global::KafkaFlow.Retry.Durable.Definitions;
    using global::KafkaFlow.Retry.Durable.Encoders;
    using global::KafkaFlow.Retry.Durable.Polling;
    using global::KafkaFlow.Retry.Durable.Repository;
    using global::KafkaFlow.Retry.Durable.Repository.Actions.Read;
    using global::KafkaFlow.Retry.Durable.Repository.Actions.Update;
    using global::KafkaFlow.Retry.Durable.Repository.Adapters;
    using global::KafkaFlow.Retry.Durable.Repository.Model;
    using global::KafkaFlow.Retry.UnitTests.KafkaFlow.Retry.Surrogate;
    using Moq;
    using Quartz;
    using Xunit;

    [ExcludeFromCodeCoverage]
    public class QueuePollingJobTests
    {
        public readonly static IEnumerable<object[]> DataTest = new List<object[]>
        {
            new object[]
            {
                new Dictionary<string, object>
                {
                    { "RetryDurableQueueRepository", null },
                    { "RetryDurableProducer", Mock.Of<IMessageProducer>() },
                    { "RetryDurablePollingDefinition", Mock.Of<IRetryDurablePollingDefinition>()},
                    { "LogHandler", Mock.Of<ILogHandler>() },
                    { "MessageHeadersAdapter", Mock.Of<IMessageHeadersAdapter>() },
                    { "MessageAdapter", Mock.Of<IMessageAdapter>() },
                    { "Utf8Encoder", Mock.Of<IUtf8Encoder>() },
                }
            },
            new object[]
            {
                new Dictionary<string, object>
                {
                    { "RetryDurableQueueRepository", Mock.Of<IRetryDurableQueueRepository>() },
                    { "RetryDurableProducer", null },
                    { "RetryDurablePollingDefinition", Mock.Of<IRetryDurablePollingDefinition>()},
                    { "LogHandler", Mock.Of<ILogHandler>() },
                    { "MessageHeadersAdapter", Mock.Of<IMessageHeadersAdapter>() },
                    { "MessageAdapter", Mock.Of<IMessageAdapter>() },
                    { "Utf8Encoder", Mock.Of<IUtf8Encoder>() },
                }
            },
            new object[]
            {
                new Dictionary<string, object>
                {
                    { "RetryDurableQueueRepository", Mock.Of<IRetryDurableQueueRepository>() },
                    { "RetryDurableProducer", Mock.Of<IMessageProducer>() },
                    { "RetryDurablePollingDefinition", null},
                    { "LogHandler", Mock.Of<ILogHandler>() },
                    { "MessageHeadersAdapter", Mock.Of<IMessageHeadersAdapter>() },
                    { "MessageAdapter", Mock.Of<IMessageAdapter>() },
                    { "Utf8Encoder", Mock.Of<IUtf8Encoder>() },
                }
            },
            new object[]
            {
                new Dictionary<string, object>
                {
                    { "RetryDurableQueueRepository", Mock.Of<IRetryDurableQueueRepository>() },
                    { "RetryDurableProducer", Mock.Of<IMessageProducer>() },
                    { "RetryDurablePollingDefinition", Mock.Of<IRetryDurablePollingDefinition>()},
                    { "LogHandler", null },
                    { "MessageHeadersAdapter", Mock.Of<IMessageHeadersAdapter>() },
                    { "MessageAdapter", Mock.Of<IMessageAdapter>() },
                    { "Utf8Encoder", Mock.Of<IUtf8Encoder>() },
                }
            },
            new object[]
            {
                new Dictionary<string, object>
                {
                    { "RetryDurableQueueRepository", Mock.Of<IRetryDurableQueueRepository>() },
                    { "RetryDurableProducer", Mock.Of<IMessageProducer>() },
                    { "RetryDurablePollingDefinition", Mock.Of<IRetryDurablePollingDefinition>()},
                    { "LogHandler", Mock.Of<ILogHandler>() },
                    { "MessageHeadersAdapter", null },
                    { "MessageAdapter", Mock.Of<IMessageAdapter>() },
                    { "Utf8Encoder", Mock.Of<IUtf8Encoder>() },
                }
            },
            new object[]
            {
                new Dictionary<string, object>
                {
                    { "RetryDurableQueueRepository", Mock.Of<IRetryDurableQueueRepository>() },
                    { "RetryDurableProducer", Mock.Of<IMessageProducer>() },
                    { "RetryDurablePollingDefinition", Mock.Of<IRetryDurablePollingDefinition>()},
                    { "LogHandler", Mock.Of<ILogHandler>() },
                    { "MessageHeadersAdapter", Mock.Of<IMessageHeadersAdapter>() },
                    { "MessageAdapter", null },
                    { "Utf8Encoder", Mock.Of<IUtf8Encoder>() },
                }
            },
            new object[]
            {
                new Dictionary<string, object>
                {
                    { "RetryDurableQueueRepository", Mock.Of<IRetryDurableQueueRepository>() },
                    { "RetryDurableProducer", Mock.Of<IMessageProducer>() },
                    { "RetryDurablePollingDefinition", Mock.Of<IRetryDurablePollingDefinition>()},
                    { "LogHandler", Mock.Of<ILogHandler>() },
                    { "MessageHeadersAdapter", Mock.Of<IMessageHeadersAdapter>() },
                    { "MessageAdapter", Mock.Of<IMessageAdapter>() },
                    { "Utf8Encoder", null },
                }
            }
        };

        private readonly IJob job = new QueuePollingJob();
        private readonly Mock<IJobExecutionContext> jobExecutionContext = new Mock<IJobExecutionContext>();
        private readonly Mock<ILogHandler> logHandler = new Mock<ILogHandler>();
        private readonly Mock<IMessageAdapter> messageAdapter = new Mock<IMessageAdapter>();
        private readonly Mock<IMessageHeadersAdapter> messageHeadersAdapter = new Mock<IMessageHeadersAdapter>();
        private readonly Mock<IMessageProducer> messageProducer = new Mock<IMessageProducer>();
        private readonly Mock<IRetryDurablePollingDefinition> retryDurablePollingDefinition = new Mock<IRetryDurablePollingDefinition>();
        private readonly Mock<IRetryDurableQueueRepository> retryDurableQueueRepository = new Mock<IRetryDurableQueueRepository>();
        private readonly Mock<IUtf8Encoder> utf8Encoder = new Mock<IUtf8Encoder>();

        [Fact]
        public async Task QueuePollingJob_Execute_ProduceMessageFailed_LogError()
        {
            // Arrange
            retryDurablePollingDefinition.Setup(d => d.CronExpression).Returns("0 0 14-6 ? * FRI-MON");
            retryDurablePollingDefinition.Setup(d => d.FetchSize).Returns(1);
            retryDurablePollingDefinition.Setup(d => d.ExpirationIntervalFactor).Returns(1);

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
                .Setup(d => d.ProduceAsync(It.IsAny<byte[]>(), It.IsAny<byte[]>(), It.IsAny<IMessageHeaders>()))
                .Throws(new Exception());

            IDictionary<string, object> data = new Dictionary<string, object>
            {
                { "RetryDurableQueueRepository", retryDurableQueueRepository.Object },
                { "RetryDurableProducer", messageProducer.Object },
                { "RetryDurablePollingDefinition", retryDurablePollingDefinition.Object},
                { "LogHandler", logHandler.Object },
                { "MessageHeadersAdapter", messageHeadersAdapter.Object },
                { "MessageAdapter", messageAdapter.Object },
                { "Utf8Encoder", utf8Encoder.Object },
            };
            var jobDataMap = new JobDataMap(data);

            jobExecutionContext.Setup(d => d.JobDetail).Returns(new JobDetailSurrogate(jobDataMap));

            // Act
            await job.Execute(jobExecutionContext.Object).ConfigureAwait(false);

            //Assert
            logHandler.Verify(d => d.Error(It.IsAny<string>(), It.IsAny<Exception>(), It.IsAny<object>()), Times.Once);
            retryDurableQueueRepository.Verify(d => d.GetRetryQueuesAsync(It.IsAny<GetQueuesInput>()), Times.Once);
            retryDurableQueueRepository.Verify(d => d.UpdateItemAsync(It.IsAny<UpdateItemStatusInput>()), Times.Exactly(2));
            messageHeadersAdapter.Verify(d => d.AdaptMessageHeadersFromRepository(It.IsAny<IList<MessageHeader>>()), Times.Once);

            retryDurableQueueRepository.Reset();
            messageProducer.Reset();
        }

        [Fact]
        public async Task QueuePollingJob_Execute_RetryDurableQueueRepositoryFailed_LogError()
        {
            // Arrange
            retryDurablePollingDefinition.Setup(d => d.CronExpression).Returns("0 0 14-6 ? * FRI-MON");
            retryDurablePollingDefinition.Setup(d => d.FetchSize).Returns(1);
            retryDurablePollingDefinition.Setup(d => d.ExpirationIntervalFactor).Returns(1);

            retryDurableQueueRepository
                .Setup(d => d.GetRetryQueuesAsync(It.IsAny<GetQueuesInput>()))
                .Throws(new RetryDurableException(new RetryError(RetryErrorCode.Consumer_BlockedException), "error"));

            IDictionary<string, object> data = new Dictionary<string, object>
            {
                { "RetryDurableQueueRepository", retryDurableQueueRepository.Object },
                { "RetryDurableProducer", messageProducer.Object },
                { "RetryDurablePollingDefinition", retryDurablePollingDefinition.Object},
                { "LogHandler", logHandler.Object },
                { "MessageHeadersAdapter", messageHeadersAdapter.Object },
                { "MessageAdapter", messageAdapter.Object },
                { "Utf8Encoder", utf8Encoder.Object },
            };
            var jobDataMap = new JobDataMap(data);

            jobExecutionContext.Setup(d => d.JobDetail).Returns(new JobDetailSurrogate(jobDataMap));

            // Act
            await job.Execute(jobExecutionContext.Object).ConfigureAwait(false);

            //Assert
            messageProducer.Verify(d => d.ProduceAsync(It.IsAny<byte[]>(), It.IsAny<byte[]>(), It.IsAny<IMessageHeaders>()), Times.Never);
            logHandler.Verify(d => d.Error(It.IsAny<string>(), It.IsAny<RetryDurableException>(), It.IsAny<object>()), Times.Once);
            retryDurableQueueRepository.Verify(d => d.GetRetryQueuesAsync(It.IsAny<GetQueuesInput>()), Times.Once);
            retryDurableQueueRepository.Verify(d => d.UpdateItemAsync(It.IsAny<UpdateItemStatusInput>()), Times.Never);
            messageHeadersAdapter.Verify(d => d.AdaptMessageHeadersFromRepository(It.IsAny<IList<MessageHeader>>()), Times.Never);
        }

        [Fact]
        public async Task QueuePollingJob_Execute_Success()
        {
            // Arrange
            retryDurablePollingDefinition.Setup(d => d.CronExpression).Returns("0 0 14-6 ? * FRI-MON");
            retryDurablePollingDefinition.Setup(d => d.FetchSize).Returns(1);
            retryDurablePollingDefinition.Setup(d => d.ExpirationIntervalFactor).Returns(1);

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
                .Setup(d => d.ProduceAsync(It.IsAny<byte[]>(), It.IsAny<byte[]>(), It.IsAny<IMessageHeaders>()));

            IDictionary<string, object> data = new Dictionary<string, object>
            {
                { "RetryDurableQueueRepository", retryDurableQueueRepository.Object },
                { "RetryDurableProducer", messageProducer.Object },
                { "RetryDurablePollingDefinition", retryDurablePollingDefinition.Object},
                { "LogHandler", logHandler.Object },
                { "MessageHeadersAdapter", messageHeadersAdapter.Object },
                { "MessageAdapter", messageAdapter.Object },
                { "Utf8Encoder", utf8Encoder.Object },
            };
            var jobDataMap = new JobDataMap(data);

            jobExecutionContext.Setup(d => d.JobDetail).Returns(new JobDetailSurrogate(jobDataMap));

            // Act
            await job.Execute(jobExecutionContext.Object).ConfigureAwait(false);

            //Assert
            messageProducer.Verify(d => d.ProduceAsync(It.IsAny<byte[]>(), It.IsAny<byte[]>(), It.IsAny<IMessageHeaders>()), Times.Once);
            retryDurableQueueRepository.Verify(d => d.GetRetryQueuesAsync(It.IsAny<GetQueuesInput>()), Times.Once);
            retryDurableQueueRepository.Verify(d => d.UpdateItemAsync(It.IsAny<UpdateItemStatusInput>()), Times.Once);
            messageHeadersAdapter.Verify(d => d.AdaptMessageHeadersFromRepository(It.IsAny<IList<MessageHeader>>()), Times.Once);
        }

        [Theory]
        [MemberData(nameof(DataTest))]
        public async Task QueuePollingJob_Execute_WithArgumentNull_ThrowsException(IDictionary<string, object> data)
        {
            // Arrange
            var jobDataMap = new JobDataMap(data);

            jobExecutionContext.Setup(d => d.JobDetail).Returns(new JobDetailSurrogate(jobDataMap));

            // Act
            Func<Task> act = async () => await job.Execute(jobExecutionContext.Object).ConfigureAwait(false);

            //Assert
            _ = await act.Should().ThrowAsync<ArgumentException>().ConfigureAwait(false);
        }
    }
}