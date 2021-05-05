namespace KafkaFlow.Retry.Durable.Repository.Actions.Create
{
    using System.Diagnostics.CodeAnalysis;
    using Dawn;

    [ExcludeFromCodeCoverage]
    public class SaveToQueueResult
    {
        public SaveToQueueResult(SaveToQueueResultStatus status)
        {
            Guard.Argument(status).NotDefault();

            this.Status = status;
        }

        public SaveToQueueResultStatus Status { get; }
    }
}
