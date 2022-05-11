namespace KafkaFlow.Retry.UnitTests.Durable.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using global::KafkaFlow.Retry.Durable.Definitions;
    using global::KafkaFlow.Retry.Durable.Encoders;
    using global::KafkaFlow.Retry.Durable.Repository;
    using global::KafkaFlow.Retry.Durable.Repository.Actions.Create;
    using global::KafkaFlow.Retry.Durable.Repository.Actions.Read;
    using global::KafkaFlow.Retry.Durable.Repository.Adapters;
    using global::KafkaFlow.Retry.Durable.Repository.Model;
    using Moq;
    using Xunit;

    public class RetryDurableQueueRepositoryTests
    {
        private readonly Mock<IMessageAdapter> mockMessageAdapter;
        private readonly Mock<IMessageHeadersAdapter> mockMessageHeadersAdapter;
        private readonly MockRepository mockRepository;
        private readonly Mock<IRetryDurableQueueRepositoryProvider> mockRetryDurableQueueRepositoryProvider;
        private readonly Mock<IUtf8Encoder> mockUtf8Encoder;
        private readonly RetryDurableQueueRepository retryDurableQueueRepository;

        public RetryDurableQueueRepositoryTests()
        {
            mockRepository = new MockRepository(MockBehavior.Strict);

            mockRetryDurableQueueRepositoryProvider = mockRepository.Create<IRetryDurableQueueRepositoryProvider>();
            var mockIUpdateRetryQueueItemHandler = mockRepository.Create<IUpdateRetryQueueItemHandler>();
            mockMessageHeadersAdapter = mockRepository.Create<IMessageHeadersAdapter>();
            mockMessageAdapter = mockRepository.Create<IMessageAdapter>();
            mockUtf8Encoder = mockRepository.Create<IUtf8Encoder>();
            var retryDurablePollingDefinition = new RetryDurablePollingDefinition(true, "0 0 0 ? * * *", 1, 1, "some_id");

            retryDurableQueueRepository = new RetryDurableQueueRepository(
                mockRetryDurableQueueRepositoryProvider.Object,
                new List<IUpdateRetryQueueItemHandler> { mockIUpdateRetryQueueItemHandler.Object },
                mockMessageHeadersAdapter.Object,
                mockMessageAdapter.Object,
                mockUtf8Encoder.Object,
                retryDurablePollingDefinition);
        }

        [Theory]
        [InlineData("key", "value")]
        [InlineData(null, "value")]
        [InlineData("", "value")]
        public async Task AddIfQueueExistsAsync_WithValidMessage_ReturnResultStatusAdded(string messageKey, string messageValue)
        {
            // Arrange
            byte[] messageKeyBytes = null;
            if (messageKey is object)
            {
                messageKeyBytes = Encoding.ASCII.GetBytes(messageKey);
            }

            var messageValueBytes = Encoding.ASCII.GetBytes(messageValue);
            AddIfQueueExistsResultStatus addedIfQueueExistsResultStatus = AddIfQueueExistsResultStatus.Added;
            Mock<IConsumerContext> mockIConsumerContext = new Mock<IConsumerContext>();
            mockIConsumerContext
                .SetupGet(c => c.Topic)
                .Returns("topic");
            mockIConsumerContext
                .SetupGet(c => c.Partition)
                .Returns(1);
            mockIConsumerContext
                .SetupGet(c => c.Offset)
                .Returns(2);
            mockIConsumerContext
                .SetupGet(c => c.MessageTimestamp)
                .Returns(new DateTime(2022, 01, 01));

            Mock<IMessageContext> mockIMessageContext = new Mock<IMessageContext>();
            mockIMessageContext
                    .Setup(c => c.ConsumerContext)
                    .Returns(mockIConsumerContext.Object);
            mockIMessageContext
                    .Setup(c => c.Message)
                    .Returns(new Message(messageKeyBytes, messageValueBytes));

            mockMessageAdapter
                .Setup(mes => mes.AdaptMessageToRepository(It.IsAny<object>()))
                .Returns(messageValueBytes);

            mockMessageHeadersAdapter
                .Setup(mes => mes.AdaptMessageHeadersToRepository(It.IsAny<IMessageHeaders>()))
                .Returns(Enumerable.Empty<MessageHeader>());

            if (messageKey is object)
            {
                mockUtf8Encoder
                    .Setup(enc => enc.Decode(It.IsAny<byte[]>()))
                    .Returns(messageKey);
            }

            mockRetryDurableQueueRepositoryProvider
                .Setup(rep => rep.CheckQueueAsync(It.IsAny<CheckQueueInput>()))
                .ReturnsAsync(new CheckQueueResult(CheckQueueResultStatus.Exists));
            mockRetryDurableQueueRepositoryProvider
                .Setup(rep => rep.SaveToQueueAsync(It.IsAny<SaveToQueueInput>()))
                .ReturnsAsync(new SaveToQueueResult(SaveToQueueResultStatus.Added));

            // Act
            var result = await retryDurableQueueRepository.AddIfQueueExistsAsync(
                mockIMessageContext.Object);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(addedIfQueueExistsResultStatus, result.Status);
            mockRepository.VerifyAll();
        }
    }
}