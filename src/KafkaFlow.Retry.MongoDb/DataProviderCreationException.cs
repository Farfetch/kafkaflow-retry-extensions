using System;
using System.Diagnostics.CodeAnalysis;

namespace KafkaFlow.Retry.MongoDb;

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