using System;
using System.Collections.Generic;
using KafkaFlow.Retry.Durable;
using KafkaFlow.Retry.Durable.Encoders;
using KafkaFlow.Retry.Durable.Repository;
using Moq;

namespace KafkaFlow.Retry.UnitTests.KafkaFlow.Retry.Durable;

public class RetryDurableConsumerLatestMiddlewareTests
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
    public void RetryDurableConsumerLatestMiddleware_Ctor_Tests(
        object logHandler,
        object retryDurableQueueRepository,
        object utf8Encoder)
    {
            // Act
            Action act = () => new RetryDurableConsumerLatestMiddleware(
                (ILogHandler)logHandler,
                (IRetryDurableQueueRepository)retryDurableQueueRepository,
                (IUtf8Encoder)utf8Encoder
                );

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }
}