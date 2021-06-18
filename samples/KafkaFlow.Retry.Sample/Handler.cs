namespace KafkaFlow.Retry.Sample
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using KafkaFlow;
    using KafkaFlow.TypedHandler;

    public class Handler : IMessageHandler<TestMessage>
    {
        public Task Handle(IMessageContext context, TestMessage message)
        {
            var key = Encoding.UTF8.GetString((byte[])context.Message.Key);
            throw new NonBlockingException("NonBlockingException");

            Console.WriteLine(
                "Partition: {0} | Offset: {1} | Message: {2} | Topic: {3}",
                context.ConsumerContext.Partition,
                context.ConsumerContext.Offset,
                message.Text,
                context.ConsumerContext.Topic);

            return Task.CompletedTask;
        }
    }
}