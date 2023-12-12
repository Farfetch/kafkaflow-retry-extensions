using System.Collections.Generic;
using KafkaFlow.Retry.Durable.Repository.Model;

namespace KafkaFlow.Retry.Durable.Repository.Adapters;

internal interface IMessageHeadersAdapter
{
    IEnumerable<MessageHeader> AdaptMessageHeadersToRepository(IMessageHeaders messageHeaders);

    IMessageHeaders AdaptMessageHeadersFromRepository(IEnumerable<MessageHeader> fromMessageHeaders);
}