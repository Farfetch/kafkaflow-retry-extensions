namespace KafkaFlow.Retry.Postgres.Model.Factories
{
    using KafkaFlow.Retry.Durable.Repository.Actions.Create;

    internal interface IRetryQueueDboFactory
    {
        RetryQueueDbo Create(SaveToQueueInput input);
    }
}
