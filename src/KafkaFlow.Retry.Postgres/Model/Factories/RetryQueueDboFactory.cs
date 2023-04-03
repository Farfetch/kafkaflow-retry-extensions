namespace KafkaFlow.Retry.Postgres.Model.Factories
{
    using System;
    using Dawn;
    using KafkaFlow.Retry.Durable.Repository.Actions.Create;
    
    internal sealed class RetryQueueDboFactory : IRetryQueueDboFactory
    {
        public RetryQueueDbo Create(SaveToQueueInput input)
        {
            Guard.Argument(input).NotNull();

            return new RetryQueueDbo
            {
                IdDomain = Guid.NewGuid(),
                SearchGroupKey = input.SearchGroupKey,
                QueueGroupKey = input.QueueGroupKey,
                CreationDate = input.CreationDate,
                LastExecution = input.LastExecution.Value,
                Status = input.QueueStatus
            };
        }
    }
}
