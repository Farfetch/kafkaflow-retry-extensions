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
            throw new AnotherCustomException("Test");

            Console.WriteLine(
                "Partition: {0} | Offset: {1} | Message: {2}",
                context.Partition,
                context.Offset,
                message.Text);

            return Task.CompletedTask;
        }
    }
}