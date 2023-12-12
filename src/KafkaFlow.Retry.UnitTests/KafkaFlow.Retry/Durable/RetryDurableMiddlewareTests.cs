using System;
using System.Collections.Generic;
using FluentAssertions;
using global::KafkaFlow.Retry.Durable;
using global::KafkaFlow.Retry.Durable.Definitions;
using Moq;
using Xunit;

namespace KafkaFlow.Retry.UnitTests.KafkaFlow.Retry.Durable;

public class RetryDurableMiddlewareTests
{
    public static IEnumerable<object[]> DataTest() => new List<object[]>
    {
        new object[]
        {
            Mock.Of<ILogHandler>(),
            null
        }
    };

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