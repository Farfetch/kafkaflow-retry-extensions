namespace KafkaFlow.Retry.UnitTests.API.Adapters.GetItems
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using FluentAssertions;
    using global::KafkaFlow.Retry.API.Adapters.GetItems;
    using global::KafkaFlow.Retry.Durable.Common;
    using global::KafkaFlow.Retry.Durable.Repository.Model;
    using Microsoft.AspNetCore.Http;
    using Xunit;

    [ExcludeFromCodeCoverage]
    public class GetItemsRequestDtoReaderTests
    {
        private const int DefaultTopItemsByQueueValue = 100;
        private const int DefaultTopQueuesValue = 10000;
        private readonly IEnumerable<RetryQueueItemStatus> DefaultItemsStatuses = new RetryQueueItemStatus[] { RetryQueueItemStatus.Waiting, RetryQueueItemStatus.InRetry };
        private readonly IEnumerable<SeverityLevel> DefaultSeverityLevels = Enumerable.Empty<SeverityLevel>();
        private readonly string httpMethod = "GET";
        private readonly IGetItemsRequestDtoReader reader = new GetItemsRequestDtoReader();
        private readonly string resourcePath = "/retry/items";

        [Fact]
        public void GetItemsRequestDtoReader_Read_Success()
        {
            // Arrange
            var requestDto = CreateHttpContext();

            // Act
            var queuesInput = reader.Read(requestDto.Request);

            // Assert
            queuesInput.ItemsStatuses.Should().BeEquivalentTo(DefaultItemsStatuses);
            queuesInput.SeverityLevels.Should().BeEquivalentTo(DefaultSeverityLevels);
            queuesInput.TopQueues.Should().Be(DefaultTopQueuesValue);
            queuesInput.TopItemsByQueue.Should().Be(DefaultTopItemsByQueueValue);
        }

        private HttpContext CreateHttpContext()
        {
            var context = new DefaultHttpContext();

            context.Request.Path = this.resourcePath;
            context.Request.Method = this.httpMethod;

            context.Response.Body = new MemoryStream();

            return context;
        }
    }
}