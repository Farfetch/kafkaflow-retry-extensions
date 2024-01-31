using System;
using System.Threading.Tasks;
using KafkaFlow.Retry.Sample.Exceptions;
using KafkaFlow.Retry.Sample.Messages;

namespace KafkaFlow.Retry.Sample.Handlers;

internal class RetryForeverTestHandler : IMessageHandler<RetryForeverTestMessage>
{
    public Task Handle(IMessageContext context, RetryForeverTestMessage message)
    {
        Console.WriteLine(
            "Partition: {0} | Offset: {1} | Message: {2}",
            context.ConsumerContext.Partition,
            context.ConsumerContext.Offset,
            message.Text);

        throw new RetryForeverTestException($"Error: {message.Text}");
    }
}