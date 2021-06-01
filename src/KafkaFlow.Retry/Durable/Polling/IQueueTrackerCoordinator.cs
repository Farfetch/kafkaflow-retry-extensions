namespace KafkaFlow.Retry.Durable.Polling
{
    using System.Threading;
    using System.Threading.Tasks;

    internal interface IQueueTrackerCoordinator
    {
        Task InitializeAsync(CancellationToken cancellationToken);

        Task ShutdownAsync(CancellationToken cancellationToken);
    }
}