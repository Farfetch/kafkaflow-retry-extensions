using System;
using System.Threading.Tasks;
using KafkaFlow;
using KafkaFlow.Retry.Sample.Exceptions;
using KafkaFlow.Retry.Sample.Messages;

namespace KafkaFlow.Retry.Sample.Handlers;

internal class RetrySimpleTestHandler : IMessageHandler<RetrySimpleTestMessage>
{
    public Task Handle(IMessageContext context, RetrySimpleTestMessage message)
    {
        Console.WriteLine(
            "Partition: {0} | Offset: {1} | Message: {2}",
            context.ConsumerContext.Partition,
            context.ConsumerContext.Offset,
            message.Text);

        throw new RetrySimpleTestException($"Error: {message.Text}");
    }
}