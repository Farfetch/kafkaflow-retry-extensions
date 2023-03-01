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
        public static IEnumerable<object[]> DataTest() => new List<object[]>
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

        [Theory]
        [MemberData(nameof(DataTest))]
        internal void RetryDurableConsumerGuaranteeOrderedMiddleware_Ctor_Tests(
            ILogHandler logHandler,
            IRetryDurableQueueRepository retryDurableQueueRepository,
            IUtf8Encoder utf8Encoder)
        {
            // Act
            Action act = () => new RetryDurableConsumerGuaranteeOrderedMiddleware(
                logHandler,
                retryDurableQueueRepository,
                utf8Encoder
                );

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }
    }
}