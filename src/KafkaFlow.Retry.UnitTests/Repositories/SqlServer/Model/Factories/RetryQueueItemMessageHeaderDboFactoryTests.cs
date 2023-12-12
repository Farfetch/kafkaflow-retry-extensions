using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using global::KafkaFlow.Retry.Durable.Repository.Model;
using global::KafkaFlow.Retry.SqlServer.Model;
using global::KafkaFlow.Retry.SqlServer.Model.Factories;
using Xunit;

namespace KafkaFlow.Retry.UnitTests.Repositories.SqlServer.Model.Factories;

public class RetryQueueItemMessageHeaderDboFactoryTests
{
    private readonly RetryQueueItemMessageHeaderDboFactory factory = new RetryQueueItemMessageHeaderDboFactory();

    private readonly IEnumerable<MessageHeader> headers = new List<MessageHeader>
    {
        new MessageHeader("key", new byte[1])
    };

    [Fact]
    public void RetryQueueItemMessageHeaderDboFactory_Create_Success()
    {
        // Act
        var result = factory.Create(headers, 1);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result.FirstOrDefault().Should().BeOfType(typeof(RetryQueueItemMessageHeaderDbo));
    }

    [Fact]
    public void RetryQueueItemMessageHeaderDboFactory_Create_WithNegativeRetryQueueItemId_ThrowsException()
    {
        // Act
        Action act = () => factory.Create(headers, -1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void RetryQueueItemMessageHeaderDboFactory_Create_WithoutHeaders_ThrowsException()
    {
        //Arrange
        IEnumerable<MessageHeader> headersNull = null;

        // Act
        Action act = () => factory.Create(headersNull, 1);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}