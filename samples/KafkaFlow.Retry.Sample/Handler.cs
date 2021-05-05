namespace KafkaFlow.Retry.Sample
{
    using System;
    using System.Threading.Tasks;
    using KafkaFlow;
    using KafkaFlow.TypedHandler;

    public class Handler : IMessageHandler<TestMessage>
    {
        public Task Handle(IMessageContext context, TestMessage message)
        {
            throw new NonBlockingException("NonBlockingException");

            Console.WriteLine(
                "Partition: {0} | Offset: {1} | Message: {2} | Topic: {3}",
                context.Partition,
                context.Offset,
                message.Text,
                context.Topic);

            return Task.CompletedTask;
        }
    }
}