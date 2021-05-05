namespace KafkaFlow.Retry.MongoDb
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    [Serializable]
    [ExcludeFromCodeCoverage]
    public class DataProviderCreationException : Exception
    {
        public DataProviderCreationException(string message, Exception innerException)
                : base(message, innerException)
        {
        }

        public DataProviderCreationException(string message)
            : base(message)
        {
        }
    }
}