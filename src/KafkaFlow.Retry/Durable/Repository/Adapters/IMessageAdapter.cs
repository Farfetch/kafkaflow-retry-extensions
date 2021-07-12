namespace KafkaFlow.Retry.Durable.Repository.Adapters
{
    internal interface IMessageAdapter
    {
        byte[] AdaptMessageFromRepository(byte[] message);

        byte[] AdaptMessageToRepository(object message);
    }
}