namespace KafkaFlow.Retry.Durable
{
    using System.Collections.Generic;
    using KafkaFlow.Retry.Durable.Repository.Model;

    internal interface IHeadersAdapter
    {
        //Headers AdaptToConfluentHeaders(IEnumerable<OriginalMessageHeader> originalHeaders);

        IEnumerable<MessageHeader> AdaptToMessageHeaders(IMessageHeaders confluentHeaders);
    }
}