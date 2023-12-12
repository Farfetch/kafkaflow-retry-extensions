using System;
using System.Diagnostics;
using System.Text.Json;

namespace KafkaFlow.Retry.IntegrationTests.Core;

internal class TraceLogHandler : ILogHandler
{
    private readonly JsonSerializerOptions _jsonSerializerOptions =
        new()
        {
            MaxDepth = 0,
            IgnoreNullValues = true,
            IgnoreReadOnlyProperties = false
        };

    public void Error(string message, Exception ex, object data)
    {
        Trace.TraceError(
            JsonSerializer.Serialize(
                new
                {
                    Message = message,
                    Exception = new
                    {
                        ex.Message,
                        ex.StackTrace
                    },
                    Data = data
                }, _jsonSerializerOptions));
    }

    public void Info(string message, object data)
    {
        Trace.TraceInformation(
            JsonSerializer.Serialize(
                new
                {
                    Message = message,
                    Data = data
                }, _jsonSerializerOptions));
    }

    public void Verbose(string message, object data)
    {
        Trace.TraceWarning(
            JsonSerializer.Serialize(
                new
                {
                    Message = message,
                    Data = data
                }, _jsonSerializerOptions));
    }

    public void Warning(string message, object data)
    {
        Trace.TraceWarning(
            JsonSerializer.Serialize(
                new
                {
                    Message = message,
                    Data = data
                }, _jsonSerializerOptions));
    }
}