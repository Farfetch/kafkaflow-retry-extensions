namespace KafkaFlow.Retry.IntegrationTests.Core
{
    using System;
    using System.Diagnostics;
    using System.Text.Json;

    internal class TraceLogHandler : ILogHandler
    {
        public void Error(string message, Exception ex, object data)
        {
            Trace.TraceError(
                JsonSerializer.Serialize(
                    new
                    {
                        Message = message,
                        Exception = ex,
                        Data = data,
                    }, new JsonSerializerOptions
                    {
                        MaxDepth = 0,
                        IgnoreNullValues = true,
                        IgnoreReadOnlyProperties = true
                    }));
        }

        public void Info(string message, object data)
        {
            Trace.TraceInformation(
                JsonSerializer.Serialize(
                    new
                    {
                        Message = message,
                        Data = data,
                    }, new JsonSerializerOptions
                    {
                        MaxDepth = 0,
                        IgnoreNullValues = true,
                        IgnoreReadOnlyProperties = true
                    }));
        }

        public void Warning(string message, object data)
        {
            Trace.TraceWarning(
                JsonSerializer.Serialize(
                    new
                    {
                        Message = message,
                        Data = data,
                    }, new JsonSerializerOptions
                    {
                        MaxDepth = 0,
                        IgnoreNullValues = true,
                        IgnoreReadOnlyProperties = true
                    }));
        }
    }
}