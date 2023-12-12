namespace KafkaFlow.Retry.Durable.Repository.Adapters;

internal interface IMessageAdapter
{
    byte[] AdaptMessageToRepository(object message);
}