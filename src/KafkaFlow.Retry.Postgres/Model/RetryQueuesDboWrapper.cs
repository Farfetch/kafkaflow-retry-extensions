namespace KafkaFlow.Retry.Postgres.Model
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    
    [ExcludeFromCodeCoverage]
    internal class RetryQueuesDboWrapper
    {
        public RetryQueuesDboWrapper()
        {
            QueuesDbos = new RetryQueueDbo[0];
            ItemsDbos = new RetryQueueItemDbo[0];
            MessagesDbos = new RetryQueueItemMessageDbo[0];
            HeadersDbos = new RetryQueueItemMessageHeaderDbo[0];
        }

        public IList<RetryQueueItemMessageHeaderDbo> HeadersDbos { get; set; }
        public IList<RetryQueueItemDbo> ItemsDbos { get; set; }
        public IList<RetryQueueItemMessageDbo> MessagesDbos { get; set; }
        public IList<RetryQueueDbo> QueuesDbos { get; set; }
    }
}
