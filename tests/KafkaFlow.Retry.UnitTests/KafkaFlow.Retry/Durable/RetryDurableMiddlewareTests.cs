using System;
using System.Collections.Generic;
using KafkaFlow.Retry.Durable;
using KafkaFlow.Retry.Durable.Definitions;
using Moq;

namespace KafkaFlow.Retry.UnitTests.KafkaFlow.Retry.Durable;

public class RetryDurableMiddlewareTests
{
    public static IEnumerable<object[]> DataTest()
    {
        return new List<object[]>
        {
            new object[]
            {
                Mock.Of<ILogHandler>(),
                null
            }
        };
    }

    [Theory]
    [MemberData(nameof(DataTest))]
    internal void RetryDurableMiddleware_Ctor_Tests(
        ILogHandler logHandler,
        RetryDurableDefinition retryDurableDefinition)
    {
        // Act
        Action act = () => new RetryDurableMiddleware(
            logHandler,
            retryDurableDefinition
        );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}