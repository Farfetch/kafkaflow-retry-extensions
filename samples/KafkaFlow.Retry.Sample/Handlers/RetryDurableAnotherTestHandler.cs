namespace KafkaFlow.Retry.Sample.Handlers
{
    using System;
    using System.Threading.Tasks;
    using KafkaFlow;
    using KafkaFlow.Retry.Sample.Exceptions;
    using KafkaFlow.Retry.Sample.Messages;
    using KafkaFlow.TypedHandler;

    internal class RetryDurableAnotherTestHandler : IMessageHandler<RetryDurableAnotherTestMessage>
    {
        public Task Handle(IMessageContext context, RetryDurableAnotherTestMessage message)
        {
            Console.WriteLine(
                "Partition: {0} | Offset: {1} | Message: {2}",
                context.ConsumerContext.Partition,
                context.ConsumerContext.Offset,
                message.Text);

            throw new RetryDurableTestException($"Error: {message.Text}");
        }
    }
}