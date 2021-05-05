namespace KafkaFlow.Retry.SqlServer.Model.Factories
{
    using KafkaFlow.Retry.Durable.Repository.Actions.Create;

    internal interface IRetryQueueDboFactory
    {
        RetryQueueDbo Create(SaveToQueueInput input);
    }
}
