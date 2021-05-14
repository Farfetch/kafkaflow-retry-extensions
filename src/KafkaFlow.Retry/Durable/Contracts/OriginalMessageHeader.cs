namespace KafkaFlow.Retry.Durable.Contracts
{
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    internal class OriginalMessageHeader
    {
        public string Key { get; set; }

        public byte[] Value { get; set; }
    }
}