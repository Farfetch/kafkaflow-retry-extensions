using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace KafkaFlow.Retry.Durable.Repository.Actions.Update;

[ExcludeFromCodeCoverage]
public class UpdateQueuesResult
{
    public UpdateQueuesResult(IEnumerable<UpdateQueueResult> results)
    {
            this.Results = results ?? new List<UpdateQueueResult>();
        }

    public IEnumerable<UpdateQueueResult> Results { get; }
}