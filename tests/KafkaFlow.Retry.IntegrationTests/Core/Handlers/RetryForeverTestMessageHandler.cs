using System.Threading.Tasks;
using KafkaFlow;
using KafkaFlow.Retry.IntegrationTests.Core.Exceptions;
using KafkaFlow.Retry.IntegrationTests.Core.Messages;
using KafkaFlow.Retry.IntegrationTests.Core.Storages;

namespace KafkaFlow.Retry.IntegrationTests.Core.Handlers;

internal class RetryForeverTestMessageHandler : IMessageHandler<RetryForeverTestMessage>
{
    public Task Handle(IMessageContext context, RetryForeverTestMessage message)
    {
            InMemoryAuxiliarStorage<RetryForeverTestMessage>.Add(message);

            if (InMemoryAuxiliarStorage<RetryForeverTestMessage>.ThrowException)
            {
                throw new RetryForeverTestException();
            }

            return Task.CompletedTask;
        }
}