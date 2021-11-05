namespace KafkaFlow.Retry.UnitTests.API.Surrogate
{
    using System.Diagnostics.CodeAnalysis;
    using global::KafkaFlow.Retry.Durable.Repository.Model;

    [ExcludeFromCodeCoverage]
    internal class DtoSurrogate
    {
        public RetryQueueStatus Text { get; set; }
    }
}