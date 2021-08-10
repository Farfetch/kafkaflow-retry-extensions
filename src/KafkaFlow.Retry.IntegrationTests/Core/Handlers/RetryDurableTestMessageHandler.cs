namespace KafkaFlow.Retry.IntegrationTests.Core.Handlers
{
    using System.Threading.Tasks;
    using KafkaFlow;
    using KafkaFlow.Retry.IntegrationTests.Core.Exceptions;
    using KafkaFlow.Retry.IntegrationTests.Core.Messages;
    using KafkaFlow.TypedHandler;

    internal class RetryDurableTestMessageHandler : IMessageHandler<RetryDurableTestMessage>
    {
        public Task Handle(IMessageContext context, RetryDurableTestMessage message)
        {
            MessageStorage.Add(message);

            if (MessageStorage.ThrowException)
            {
                throw new RetryDurableTestException();
            }

            return Task.CompletedTask;
        }
    }
}