namespace KafkaFlow.Retry.Durable.Repository.Adapters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using KafkaFlow.Retry.Durable.Common;
    using KafkaFlow.Retry.Durable.Repository.Model;

    internal class HeadersAdapter : IHeadersAdapter
    {
        public IMessageHeaders AdaptToConfluentHeaders(Guid queueId, RetryQueueItem item) // TODO: the headers are not from confluent. are a type declared at KafkaFlow
        {
            var messageHeaders = new MessageHeaders();

            if (item.Message.Headers != null)
            {
                foreach (var header in item.Message.Headers)
                {
                    messageHeaders.Add(header.Key, header.Value);
                }
            }

            messageHeaders.Add(KafkaRetryDurableConstants.AttemptsCount, item.AttemptsCount.ToString().StringToByteArray());
            messageHeaders.Add(KafkaRetryDurableConstants.QueueId, queueId.ToString().StringToByteArray());
            messageHeaders.Add(KafkaRetryDurableConstants.ItemId, item.Id.ToString().StringToByteArray());
            messageHeaders.Add(KafkaRetryDurableConstants.Sort, item.Sort.ToString().StringToByteArray());

            return messageHeaders;
        }

        public IEnumerable<MessageHeader> AdaptToMessageHeaders(IMessageHeaders messageHeaders)
        {
            return messageHeaders?.Select(h => new MessageHeader(h.Key, h.Value));
        }
    }
}