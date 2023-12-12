using System.Threading.Tasks;
using KafkaFlow;
using KafkaFlow.Retry.IntegrationTests.Core.Exceptions;
using KafkaFlow.Retry.IntegrationTests.Core.Messages;
using KafkaFlow.Retry.IntegrationTests.Core.Storages;

namespace KafkaFlow.Retry.IntegrationTests.Core.Handlers;

internal class RetryDurableTestMessageHandler : IMessageHandler<RetryDurableTestMessage>
{
    private readonly ILogHandler logHandler;

    public RetryDurableTestMessageHandler(ILogHandler logHandler)
    {
            this.logHandler = logHandler;
        }

    public Task Handle(IMessageContext context, RetryDurableTestMessage message)
    {
            InMemoryAuxiliarStorage<RetryDurableTestMessage>.Add(message);

            if (InMemoryAuxiliarStorage<RetryDurableTestMessage>.ThrowException)
            {
                throw new RetryDurableTestException();
            }

            return Task.CompletedTask;
        }
}