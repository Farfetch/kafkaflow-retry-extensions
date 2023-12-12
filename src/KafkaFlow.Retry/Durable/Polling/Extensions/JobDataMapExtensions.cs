using Dawn;
using Quartz;

namespace KafkaFlow.Retry.Durable.Polling.Extensions;

internal static class JobDataMapExtensions
{
    public static string GetValidStringValue(this JobDataMap jobDataMap, string key, string jobName)
    {
        var stringValue = jobDataMap.GetValidValue<string>(key, jobName);

        Guard.Argument(stringValue).NotEmpty($"Argument {key} can't be an empty string for the job {jobName}.");

        return stringValue;
    }

    public static T GetValidValue<T>(this JobDataMap jobDataMap, string key, string jobName) where T : class
    {
        jobDataMap.TryGetValue(key, out var objValue);

        var value = objValue as T;

        Guard.Argument(value).NotNull($"Argument {key} is required for the job {jobName}.");

        return value;
    }
}