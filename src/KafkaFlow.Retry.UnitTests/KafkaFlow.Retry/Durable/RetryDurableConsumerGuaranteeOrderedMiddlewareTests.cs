namespace KafkaFlow.Retry.UnitTests.KafkaFlow.Retry.Durable
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using global::KafkaFlow.Retry.Durable;
    using global::KafkaFlow.Retry.Durable.Encoders;
    using global::KafkaFlow.Retry.Durable.Repository;
    using Moq;
    using Xunit;

    public class RetryDurableConsumerGuaranteeOrderedMiddlewareTests
    {
        public static readonly IEnumerable<object[]> DataTest = new List<object[]>
        {
            new object[]
            {
                null,
                Mock.Of<IRetryDurableQueueRepository>(),
                Mock.Of<IUtf8Encoder>()
            },
            new object[]
            {
                Mock.Of<ILogHandler>(),
                null,
                Mock.Of<IUtf8Encoder>()
            },
            new object[]
            {
                Mock.Of<ILogHandler>(),
                Mock.Of<IRetryDurableQueueRepository>(),
                null
            }
        };

        private readonly Mock<ILogHandler> logHandler = new Mock<ILogHandler>();
        private readonly Mock<IMessageContext> messageContext = new Mock<IMessageContext>();
        private readonly Mock<IRetryDurableQueueRepository> retryDurableQueueRepository = new Mock<IRetryDurableQueueRepository>();
        private readonly Mock<IUtf8Encoder> utf8Encoder = new Mock<IUtf8Encoder>();

        [Theory(Skip = "Todo")]
        [MemberData(nameof(DataTest))]
        public void RetryDurableConsumerGuaranteeOrderedMiddleware_Ctor_Tests(
            object logHandler,
            object retryDurableQueueRepository,
            object utf8Encoder)
        {
            // Act
            Action act = () => new RetryDurableConsumerGuaranteeOrderedMiddleware(
                (ILogHandler)logHandler,
                (IRetryDurableQueueRepository)retryDurableQueueRepository,
                (IUtf8Encoder)utf8Encoder
                );

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }
    }
}