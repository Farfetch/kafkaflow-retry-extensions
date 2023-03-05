namespace KafkaFlow.Retry.Durable.Polling
{
    using System.Collections.Generic;

    internal interface IJobDataProvidersFactory
    {
        IEnumerable<IJobDataProvider> Create(IMessageProducer retryDurableMessageProducer, ILogHandler logHandler);
    }
}