namespace KafkaFlow.Retry.Durable.Polling
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    internal interface IQueueTrackerCoordinator : IDisposable
    {
        Task InitializeAsync(KafkaRetryDurableDefinition kafkaRetryDurableDefinition, CancellationToken cancellationToken);

        Task ShutdownAsync(CancellationToken cancellationToken);
    }
}