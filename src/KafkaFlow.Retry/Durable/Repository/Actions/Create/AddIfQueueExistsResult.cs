using System.Diagnostics.CodeAnalysis;
using Dawn;

namespace KafkaFlow.Retry.Durable.Repository.Actions.Create;

[ExcludeFromCodeCoverage]
public class AddIfQueueExistsResult
{
    public AddIfQueueExistsResult(AddIfQueueExistsResultStatus status)
    {
        Guard.Argument(status).NotDefault();

        Status = status;
    }

    public AddIfQueueExistsResultStatus Status { get; }
}