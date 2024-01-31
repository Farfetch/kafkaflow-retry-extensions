using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using KafkaFlow.Retry.IntegrationTests.Core.Messages;

namespace KafkaFlow.Retry.IntegrationTests.Core.Storages;

internal static class InMemoryAuxiliarStorage<T> where T : ITestMessage
{
    private const int TimeoutSec = 90;
    private static readonly ConcurrentBag<T> s_message = new();

    public static bool ThrowException { get; set; }

    public static void Add(T message)
    {
        s_message.Add(message);
    }

    public static async Task AssertCountMessageAsync(T message, int count)
    {
        var start = DateTime.Now;

        while (s_message.Count(x => x.Key == message.Key && x.Value == message.Value) != count)
        {
            if (DateTime.Now.Subtract(start).TotalSeconds > TimeoutSec && !Debugger.IsAttached)
            {
                Assert.Fail("Message not received.");
                return;
            }

            await Task.Delay(100);
        }
    }

    public static void Clear()
    {
        s_message.Clear();
    }
}