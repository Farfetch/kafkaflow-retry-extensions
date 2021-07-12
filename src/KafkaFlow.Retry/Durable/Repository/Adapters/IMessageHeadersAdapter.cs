namespace KafkaFlow.Retry.Durable.Repository.Adapters
{
    using System.Collections.Generic;
    using KafkaFlow.Retry.Durable.Repository.Model;

    internal interface IMessageHeadersAdapter
    {
        IEnumerable<MessageHeader> AdaptMessageHeadersToRepository(IMessageHeaders messageHeaders);

        IMessageHeaders AdaptMessageHeadersFromRepository(IEnumerable<MessageHeader> fromMessageHeaders);
    }
}