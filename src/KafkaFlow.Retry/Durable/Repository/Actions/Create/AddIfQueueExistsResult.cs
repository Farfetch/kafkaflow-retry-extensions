namespace KafkaFlow.Retry.Durable.Repository.Actions.Create
{
    using System.Diagnostics.CodeAnalysis;
    using Dawn;

    [ExcludeFromCodeCoverage]
    public class AddIfQueueExistsResult
    {
        public AddIfQueueExistsResult(AddIfQueueExistsResultStatus status)
        {
            Guard.Argument(status).NotDefault();

            this.Status = status;
        }

        public AddIfQueueExistsResultStatus Status { get; }
    }
}
