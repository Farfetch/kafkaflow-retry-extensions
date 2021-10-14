namespace KafkaFlow.Retry.UnitTests.KafkaFlow.Retry.Durable
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using global::KafkaFlow.Retry.Durable;
    using global::KafkaFlow.Retry.Durable.Definitions;
    using global::KafkaFlow.Retry.Durable.Repository;
    using Moq;
    using Xunit;

    public class RetryDurableMiddlewareTests
    {
        public static readonly IEnumerable<object[]> DataTest = new List<object[]>
        {
            new object[]
            {
                null,
                Mock.Of<RetryDurableDefinition>()
            },
            new object[]
            {
                Mock.Of<ILogHandler>(),
                null
            }
        };

        private readonly Mock<ILogHandler> logHandler = new Mock<ILogHandler>();
        private readonly Mock<IMessageContext> messageContext = new Mock<IMessageContext>();
        private readonly Mock<IRetryDurableQueueRepository> retryDurableQueueRepository = new Mock<IRetryDurableQueueRepository>();

        [Theory(Skip = "I will discuss with the team")]
        [MemberData(nameof(DataTest))]
        public void RetryDurableMiddleware_Ctor_Tests(
            object logHandler,
            object retryDurableDefinition)
        {
            // Act
            Action act = () => new RetryDurableMiddleware(
                (ILogHandler)logHandler,
                (RetryDurableDefinition)retryDurableDefinition
                );

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }
    }
}