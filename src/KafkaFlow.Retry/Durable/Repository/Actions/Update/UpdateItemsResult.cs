namespace KafkaFlow.Retry.Durable.Repository.Actions.Update
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    public class UpdateItemsResult
    {
        public UpdateItemsResult(IEnumerable<UpdateItemResult> results)
        {
            this.Results = results ?? new List<UpdateItemResult>();
        }

        public IEnumerable<UpdateItemResult> Results { get; }
    }
}
