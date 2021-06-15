namespace KafkaFlow.Retry.Durable.Repository.Adapters
{
    using System.Collections.Generic;
    using KafkaFlow.Retry.Durable.Repository.Model;

    internal interface IMessageHeadersAdapter
    {
        IEnumerable<MessageHeader> AdaptFromKafkaFlowMessageHeaders(IMessageHeaders messageHeaders);

        IMessageHeaders AdaptToKafkaFlowMessageHeaders(IEnumerable<MessageHeader> fromMessageHeaders);
    }
}