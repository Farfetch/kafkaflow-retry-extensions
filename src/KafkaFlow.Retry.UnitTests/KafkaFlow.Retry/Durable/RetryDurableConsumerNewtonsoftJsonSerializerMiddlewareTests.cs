using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using global::KafkaFlow.Retry.Durable;
using global::KafkaFlow.Retry.Durable.Serializers;
using Moq;
using Xunit;

namespace KafkaFlow.Retry.UnitTests.KafkaFlow.Retry.Durable;

public class RetryDurableConsumerNewtonsoftJsonSerializerMiddlewareTests
{
    public static IEnumerable<object[]> DataTest()
    {
        yield return new object[]
        {
            null,
            typeof(Type)
        };
        yield return new object[]
        {
            Mock.Of<INewtonsoftJsonSerializer>(),
            null
        };
    }

    [Theory]
    [MemberData(nameof(DataTest))]
    internal void RetryDurableConsumerNewtonsoftJsonSerializerMiddleware_Ctor_Tests(
        INewtonsoftJsonSerializer newtonsoftJsonSerializer,
        Type type)
    {
        // Act
        Action act = () => new RetryDurableConsumerNewtonsoftJsonSerializerMiddleware(
            newtonsoftJsonSerializer,
            type);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    internal async Task RetryDurableConsumerNewtonsoftJsonSerializerMiddleware_Invoke_Tests()
    {
        // Arrange
        var deserialized = new { a = 1 };

        var mockINewtonsoftJsonSerializer = new Mock<INewtonsoftJsonSerializer>();
        mockINewtonsoftJsonSerializer
            .Setup(x => x.DeserializeObject(It.IsAny<string>(), It.IsAny<Type>()))
            .Returns(deserialized);

        var mockIMessageContext = new Mock<IMessageContext>();

        var newtonsoftJsonSerializerMiddleware = new RetryDurableConsumerNewtonsoftJsonSerializerMiddleware(
            mockINewtonsoftJsonSerializer.Object,
            typeof(Type));

        // Act
        await newtonsoftJsonSerializerMiddleware.Invoke(mockIMessageContext.Object, _ => Task.CompletedTask).ConfigureAwait(false);

        // Assert
        mockIMessageContext.Verify(c => c.SetMessage(null, deserialized), Times.Once);
    }
}