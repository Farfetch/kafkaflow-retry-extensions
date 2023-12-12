using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace KafkaFlow.Retry.Durable.Repository.Actions.Update;

[ExcludeFromCodeCoverage]
public class UpdateItemsResult
{
    public UpdateItemsResult(IEnumerable<UpdateItemResult> results)
    {
        Results = results ?? new List<UpdateItemResult>();
    }

    public IEnumerable<UpdateItemResult> Results { get; }
}