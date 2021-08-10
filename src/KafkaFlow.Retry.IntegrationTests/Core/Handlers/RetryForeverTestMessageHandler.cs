namespace KafkaFlow.Retry.IntegrationTests.Core.Handlers
{
    using System.Threading.Tasks;
    using KafkaFlow;
    using KafkaFlow.Retry.IntegrationTests.Core.Exceptions;
    using KafkaFlow.Retry.IntegrationTests.Core.Messages;
    using KafkaFlow.TypedHandler;

    internal class RetryForeverTestMessageHandler : IMessageHandler<RetryForeverTestMessage>
    {
        public Task Handle(IMessageContext context, RetryForeverTestMessage message)
        {
            MessageStorage.Add(message);

            throw new RetryForeverTestException();
        }
    }
}