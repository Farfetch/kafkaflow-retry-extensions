using KafkaFlow.Retry.Durable.Repository.Actions.Create;

namespace KafkaFlow.Retry.SqlServer.Model.Factories;

internal interface IRetryQueueDboFactory
{
    RetryQueueDbo Create(SaveToQueueInput input);
}