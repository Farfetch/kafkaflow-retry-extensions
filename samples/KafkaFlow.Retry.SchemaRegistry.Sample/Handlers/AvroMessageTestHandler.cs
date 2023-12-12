using System;
using System.Threading.Tasks;
using SchemaRegistry;
using KafkaFlow.Retry.SchemaRegistry.Sample.Exceptions;

namespace KafkaFlow.Retry.SchemaRegistry.Sample.Handlers;

public class AvroMessageTestHandler : IMessageHandler<AvroLogMessage>
{
    public Task Handle(IMessageContext context, AvroLogMessage message)
    {
        Console.WriteLine(
            "Partition: {0} | Offset: {1} | Message: {2} | Avro",
            context.ConsumerContext.Partition,
            context.ConsumerContext.Offset,
            message.Severity.ToString());

        throw new RetryDurableTestException($"Error: {message.Severity}");
    }
}