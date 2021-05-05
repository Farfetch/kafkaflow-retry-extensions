namespace KafkaFlow.Retry.Durable.Repository.Actions.Update
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    public class UpdateQueuesResult
    {
        public UpdateQueuesResult(IEnumerable<UpdateQueueResult> results)
        {
            this.Results = results ?? new List<UpdateQueueResult>();
        }

        public IEnumerable<UpdateQueueResult> Results { get; }
    }
}
