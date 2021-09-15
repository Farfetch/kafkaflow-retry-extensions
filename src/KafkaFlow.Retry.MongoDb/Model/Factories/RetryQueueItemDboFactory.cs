namespace KafkaFlow.Retry.MongoDb.Model.Factories
{
    using System;
    using Dawn;
    using KafkaFlow.Retry.Durable.Repository.Actions.Create;
    using KafkaFlow.Retry.MongoDb.Adapters.Interfaces;

    internal class RetryQueueItemDboFactory
    {
        private readonly IMessageAdapter messageAdapter;

        public RetryQueueItemDboFactory(IMessageAdapter messageAdapter)
        {
            Guard.Argument(messageAdapter, nameof(messageAdapter)).NotNull();

            this.messageAdapter = messageAdapter;
        }

        public RetryQueueItemDbo Create(SaveToQueueInput input, Guid queueId, int sort = 0)
        {
            Guard.Argument(input, nameof(input)).NotNull();
            Guard.Argument(queueId).NotDefault();
            Guard.Argument(sort, nameof(sort)).NotNegative();

            return new RetryQueueItemDbo
            {
                CreationDate = input.CreationDate,
                LastExecution = input.LastExecution,
                ModifiedStatusDate = input.ModifiedStatusDate,
                AttemptsCount = input.AttemptsCount,
                Message = this.messageAdapter.Adapt(input.Message),
                RetryQueueId = queueId,
                Sort = sort,
                Status = input.ItemStatus,
                SeverityLevel = input.SeverityLevel,
                Description = input.Description
            };
        }
    }
}