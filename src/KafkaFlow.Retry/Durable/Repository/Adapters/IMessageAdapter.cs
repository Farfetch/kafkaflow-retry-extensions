namespace KafkaFlow.Retry.Durable.Repository.Adapters
{
    using System;

    internal interface IMessageAdapter
    {
        byte[] AdaptFromKafkaFlowMessage(object message);

        object AdaptToKafkaFlowMessage(byte[] message, Type type);
    }
}