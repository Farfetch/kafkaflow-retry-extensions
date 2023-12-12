using System;
using System.Collections.Generic;
using System.Linq;
using KafkaFlow.Retry.Durable.Repository.Model;
using KafkaFlow.Retry.SqlServer.Model;
using KafkaFlow.Retry.SqlServer.Model.Factories;

namespace KafkaFlow.Retry.UnitTests.Repositories.SqlServer.Model.Factories;

public class RetryQueueItemMessageHeaderDboFactoryTests
{
    private readonly RetryQueueItemMessageHeaderDboFactory _factory = new();

    private readonly IEnumerable<MessageHeader> _headers = new List<MessageHeader>
    {
        new("key", new byte[1])
    };

    [Fact]
    public void RetryQueueItemMessageHeaderDboFactory_Create_Success()
    {
        // Act
        var result = _factory.Create(_headers, 1);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result.FirstOrDefault().Should().BeOfType(typeof(RetryQueueItemMessageHeaderDbo));
    }

    [Fact]
    public void RetryQueueItemMessageHeaderDboFactory_Create_WithNegativeRetryQueueItemId_ThrowsException()
    {
        // Act
        Action act = () => _factory.Create(_headers, -1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void RetryQueueItemMessageHeaderDboFactory_Create_WithoutHeaders_ThrowsException()
    {
        //Arrange
        IEnumerable<MessageHeader> headersNull = null;

        // Act
        Action act = () => _factory.Create(headersNull, 1);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}