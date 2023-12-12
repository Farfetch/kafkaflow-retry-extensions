using System.Collections.Generic;
using System.Linq;
using KafkaFlow.Retry.API.Adapters.Common.Parsers;
using KafkaFlow.Retry.API.Dtos;
using KafkaFlow.Retry.Durable.Common;
using KafkaFlow.Retry.Durable.Repository.Model;
using Microsoft.AspNetCore.Http;

namespace KafkaFlow.Retry.API.Adapters.GetItems;

internal class GetItemsRequestDtoReader : IGetItemsRequestDtoReader
{
    private const int DefaultTopItemsByQueueValue = 100;
    private const int DefaultTopQueuesValue = 10000;
    private readonly IEnumerable<RetryQueueItemStatus> _defaultItemsStatuses = new RetryQueueItemStatus[] { RetryQueueItemStatus.Waiting, RetryQueueItemStatus.InRetry };
    private readonly IEnumerable<SeverityLevel> _defaultSeverityLevels = Enumerable.Empty<SeverityLevel>();

    private readonly EnumParser<SeverityLevel> _severitiesParser;
    private readonly EnumParser<RetryQueueItemStatus> _statusesParser;

    public GetItemsRequestDtoReader()
    {
        _statusesParser = new EnumParser<RetryQueueItemStatus>();
        _severitiesParser = new EnumParser<SeverityLevel>();
    }

    public GetItemsRequestDto Read(HttpRequest request)
    {
        var statusIds = request.ReadQueryParams("status");
        var severityIds = request.ReadQueryParams("severitylevel");
        var topQueues = request.ReadQueryParams("topqueues");
        var topItemsByQueue = request.ReadQueryParams("topitemsbyqueue");

        return new GetItemsRequestDto()
        {
            ItemsStatuses = _statusesParser.Parse(statusIds, _defaultItemsStatuses),
            SeverityLevels = _severitiesParser.Parse(severityIds, _defaultSeverityLevels),
            TopQueues = int.TryParse(topQueues.LastOrDefault(), out int parsedTopQueues) ? parsedTopQueues : DefaultTopQueuesValue,
            TopItemsByQueue = int.TryParse(topItemsByQueue.LastOrDefault(), out int parsedTopItemsByQueue) ? parsedTopItemsByQueue : DefaultTopItemsByQueueValue
        };
    }
}