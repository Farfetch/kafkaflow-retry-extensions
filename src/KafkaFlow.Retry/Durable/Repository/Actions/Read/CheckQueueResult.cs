using System.Diagnostics.CodeAnalysis;
using Dawn;

namespace KafkaFlow.Retry.Durable.Repository.Actions.Read;

[ExcludeFromCodeCoverage]
public class CheckQueueResult
{
    public CheckQueueResult(CheckQueueResultStatus status)
    {
            Guard.Argument(status).NotDefault();

            this.Status = status;
        }

    public CheckQueueResultStatus Status { get; }
}