using System.Diagnostics.CodeAnalysis;
using Dawn;

namespace KafkaFlow.Retry.Durable.Repository.Actions.Create;

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