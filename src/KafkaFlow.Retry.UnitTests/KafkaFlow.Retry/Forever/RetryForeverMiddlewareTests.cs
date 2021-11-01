namespace KafkaFlow.Retry.UnitTests.KafkaFlow.Retry.Forever
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using global::KafkaFlow.Retry.Forever;
    using Moq;
    using Xunit;

    public class RetryForeverMiddlewareTests
    {
        public static readonly IEnumerable<object[]> DataTest = new List<object[]>
        {
            new object[]
            {
                null,
                Mock.Of<RetryForeverDefinition>()
            },
            new object[]
            {
                Mock.Of<ILogHandler>(),
                null
            }
        };

        [Theory(Skip = "Todo")]
        [MemberData(nameof(DataTest))]
        public void RetryForeverMiddleware_Ctor_Tests(
            object logHandler,
            object retryForeverDefinition)
        {
            // Act
            Action act = () => new RetryForeverMiddleware(
                (ILogHandler)logHandler,
                (RetryForeverDefinition)retryForeverDefinition
                );

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }
    }
}