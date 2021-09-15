namespace KafkaFlow.Retry.UnitTests.KafkaFlow.Retry.Simple
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using global::KafkaFlow.Retry.Simple;
    using Moq;
    using Xunit;

    public class RetrySimpleMiddlewareTests
    {
        public static readonly IEnumerable<object[]> DataTest = new List<object[]>
        {
            new object[]
            {
                null,
                Mock.Of<IRetrySimpleDefinition>()
            },
            new object[]
            {
                Mock.Of<ILogHandler>(),
                null
            }
        };

        [Theory]
        [MemberData(nameof(DataTest))]
        public void RetrySimpleMiddleware_Ctor_Tests(
            object logHandler,
            object retryForeverDefinition)
        {
            // Act
            Action act = () => new RetrySimpleMiddleware(
                (ILogHandler)logHandler,
                (IRetrySimpleDefinition)retryForeverDefinition
                );

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }
    }
}