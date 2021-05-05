namespace KafkaFlow.Retry.SqlServer.Model.Factories
{
    using System;
    using Dawn;
    using KafkaFlow.Retry.Durable.Repository.Actions.Create;

    internal sealed class RetryQueueItemDboFactory : IRetryQueueItemDboFactory
    {
        public RetryQueueItemDbo Create(SaveToQueueInput input, long retryQueueId, Guid retryQueueDomainId, int sort = 0)
        {
            Guard.Argument(input, nameof(input)).NotNull();
            Guard.Argument(retryQueueId, nameof(retryQueueId)).Positive();
            Guard.Argument(retryQueueDomainId, nameof(retryQueueDomainId)).NotDefault();
            Guard.Argument(sort, nameof(sort)).NotNegative();

            return new RetryQueueItemDbo
            {
                IdDomain = Guid.NewGuid(),
                CreationDate = input.CreationDate,
                LastExecution = input.LastExecution,
                ModifiedStatusDate = input.ModifiedStatusDate,
                AttemptsCount = input.AttemptsCount,
                RetryQueueId = retryQueueId,
                DomainRetryQueueId = retryQueueDomainId,
                Sort = sort,
                Status = input.ItemStatus,
                SeverityLevel = input.SeverityLevel,
                Description = input.Description
            };
        }
    }
}