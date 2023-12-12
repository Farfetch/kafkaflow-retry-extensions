using System.Diagnostics.CodeAnalysis;
using Dawn;

namespace KafkaFlow.Retry.Durable.Repository.Actions.Read;

[ExcludeFromCodeCoverage]
public class QueueNewestItemsResult
{
    public QueueNewestItemsResult(QueueNewestItemsResultStatus status)
    {
            Guard.Argument(status, nameof(status)).NotDefault();

            Status = status;
        }

    public QueueNewestItemsResultStatus Status { get; }
}