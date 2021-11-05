namespace KafkaFlow.Retry.UnitTests.KafkaFlow.Retry.Durable
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using FluentAssertions;
    using global::KafkaFlow.Retry.Durable;
    using global::KafkaFlow.Retry.Durable.Encoders;
    using global::KafkaFlow.Retry.Durable.Repository;
    using Moq;
    using Xunit;

    [ExcludeFromCodeCoverage]
    public class RetryDurableConsumerValidationMiddlewareTests
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

        [Theory]
        [MemberData(nameof(DataTest))]
        public void RetryDurableConsumerValidationMiddleware_Ctor_Tests(
            object logHandler,
            object retryDurableQueueRepository,
            object utf8Encoder)
        {
            // Act
            Action act = () => new RetryDurableConsumerValidationMiddleware(
                (ILogHandler)logHandler,
                (IRetryDurableQueueRepository)retryDurableQueueRepository,
                (IUtf8Encoder)utf8Encoder
                );

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }
    }
}