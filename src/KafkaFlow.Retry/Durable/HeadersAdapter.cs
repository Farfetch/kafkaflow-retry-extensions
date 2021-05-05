namespace KafkaFlow.Retry.Durable
{
    using System.Collections.Generic;
    using System.Linq;
    using KafkaFlow.Retry.Durable.Repository.Model;

    internal class HeadersAdapter : IHeadersAdapter
    {
        //public Headers AdaptToConfluentHeaders(IEnumerable<OriginalMessageHeader> originalHeaders)
        //{
        //    Headers headers = new Headers();

        // if (originalHeaders is null) { return headers; }

        // foreach (var h in originalHeaders) { headers.Add(new Header(h.Key, h.Value)); }

        //    return headers;
        //}

        public IEnumerable<MessageHeader> AdaptToMessageHeaders(IMessageHeaders confluentHeaders)
        {
            return confluentHeaders?.Select(h => new MessageHeader(h.Key, h.Value));
        }
    }
}