using System.Collections.Generic;

namespace KafkaFlow.Retry.Durable.Polling;

internal interface IJobDataProvidersFactory
{
    IEnumerable<IJobDataProvider> Create(IMessageProducer retryDurableMessageProducer, ILogHandler logHandler);
}