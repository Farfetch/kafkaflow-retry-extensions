namespace KafkaFlow.Retry.Durable.Repository.Adapters
{
    using System.Collections.Generic;
    using System.Linq;
    using KafkaFlow.Retry.Durable.Repository.Model;

    internal class MessageHeadersAdapter : IMessageHeadersAdapter
    {
        public IMessageHeaders AdaptMessageHeadersFromRepository(IEnumerable<MessageHeader> fromMessageHeaders)
        {
            var toMessageHeaders = new MessageHeaders();

            if (fromMessageHeaders is null)
            {
                return toMessageHeaders;
            }

            foreach (var header in fromMessageHeaders)
            {
                toMessageHeaders.Add(header.Key, header.Value);
            }

            return toMessageHeaders;
        }

        public IEnumerable<MessageHeader> AdaptMessageHeadersToRepository(IMessageHeaders messageHeaders)
        {
            if (messageHeaders is null)
            {
                return Enumerable.Empty<MessageHeader>();
            }

            return messageHeaders.Select(h => new MessageHeader(h.Key, h.Value)).ToList();
        }
    }
}