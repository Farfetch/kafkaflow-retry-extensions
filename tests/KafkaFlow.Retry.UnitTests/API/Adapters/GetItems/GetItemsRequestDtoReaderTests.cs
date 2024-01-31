using System.Collections.Generic;
using System.IO;
using System.Linq;
using KafkaFlow.Retry.API.Adapters.GetItems;
using KafkaFlow.Retry.Durable.Common;
using KafkaFlow.Retry.Durable.Repository.Model;
using Microsoft.AspNetCore.Http;

namespace KafkaFlow.Retry.UnitTests.API.Adapters.GetItems;

public class GetItemsRequestDtoReaderTests
{
    private const int DefaultTopItemsByQueueValue = 100;
    private const int DefaultTopQueuesValue = 10000;

    private readonly IEnumerable<RetryQueueItemStatus> _defaultItemsStatuses =
        new[] { RetryQueueItemStatus.Waiting, RetryQueueItemStatus.InRetry };

    private readonly IEnumerable<SeverityLevel> _defaultSeverityLevels = Enumerable.Empty<SeverityLevel>();
    private readonly string _httpMethod = "GET";
    private readonly IGetItemsRequestDtoReader _reader = new GetItemsRequestDtoReader();
    private readonly string _resourcePath = "/retry/items";

    [Fact]
    public void GetItemsRequestDtoReader_Read_Success()
    {
        // Arrange
        var requestDto = CreateHttpContext();

        // Act
        var queuesInput = _reader.Read(requestDto.Request);

        // Assert
        queuesInput.ItemsStatuses.Should().BeEquivalentTo(_defaultItemsStatuses);
        queuesInput.SeverityLevels.Should().BeEquivalentTo(_defaultSeverityLevels);
        queuesInput.TopQueues.Should().Be(DefaultTopQueuesValue);
        queuesInput.TopItemsByQueue.Should().Be(DefaultTopItemsByQueueValue);
    }

    private HttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();

        context.Request.Path = _resourcePath;
        context.Request.Method = _httpMethod;

        context.Response.Body = new MemoryStream();

        return context;
    }
}