namespace KafkaFlow.Retry.UnitTests.KafkaFlow.Retry.Durable
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using global::KafkaFlow.Retry.Durable;
    using global::KafkaFlow.Retry.Durable.Definitions;
    using global::KafkaFlow.Retry.Durable.Encoders;
    using global::KafkaFlow.Retry.Durable.Repository;
    using Moq;
    using Xunit;

    public class RetryDurableConsumerValidationMiddlewareTests
    {
        public static readonly IEnumerable<object[]> DataTest = new List<object[]>
        {
            new object[]
            {
                null,
                Mock.Of<IRetryDurableQueueRepository>(),
                Mock.Of<IRetryDurableDefinition>(),
                Mock.Of<IUtf8Encoder>()
            },
            new object[]
            {
                Mock.Of<ILogHandler>(),
                null,
                Mock.Of<IRetryDurableDefinition>(),
                Mock.Of<IUtf8Encoder>()
            },
            new object[]
            {
                Mock.Of<ILogHandler>(),
                Mock.Of<IRetryDurableQueueRepository>(),
                null,
                Mock.Of<IUtf8Encoder>()
            },
            new object[]
            {
                Mock.Of<ILogHandler>(),
                Mock.Of<IRetryDurableQueueRepository>(),
                Mock.Of<IRetryDurableDefinition>(),
                null
            }
        };

        private readonly Mock<ILogHandler> logHandler = new Mock<ILogHandler>();
        private readonly Mock<IMessageContext> messageContext = new Mock<IMessageContext>();
        private readonly Mock<IRetryDurableQueueRepository> retryDurableQueueRepository = new Mock<IRetryDurableQueueRepository>();
        private readonly Mock<IUtf8Encoder> utf8Encoder = new Mock<IUtf8Encoder>();

        [Theory(Skip = "Todo")]
        [MemberData(nameof(DataTest))]
        public void RetryDurableConsumerValidationMiddleware_Ctor_Tests(
            object logHandler,
            object retryDurableQueueRepository,
            object retryDurableDefinition,
            object utf8Encoder)
        {
            // Act
            Action act = () => new RetryDurableConsumerValidationMiddleware(
                (ILogHandler)logHandler,
                (IRetryDurableQueueRepository)retryDurableQueueRepository,
                (IRetryDurableDefinition)retryDurableDefinition,
                (IUtf8Encoder)utf8Encoder
                );

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }
    }
}