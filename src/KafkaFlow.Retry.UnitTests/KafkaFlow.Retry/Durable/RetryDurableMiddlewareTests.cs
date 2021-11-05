namespace KafkaFlow.Retry.UnitTests.KafkaFlow.Retry.Durable
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using FluentAssertions;
    using global::KafkaFlow.Retry.Durable;
    using global::KafkaFlow.Retry.Durable.Definitions;
    using Moq;
    using Xunit;

    [ExcludeFromCodeCoverage]
    public class RetryDurableMiddlewareTests
    {
        public static readonly IEnumerable<object[]> DataTest = new List<object[]>
        {
            new object[]
            {
                Mock.Of<ILogHandler>(),
                null
            }
        };

        [Theory]
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