namespace KafkaFlow.Retry.API
{
    using System;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Http;

    internal static class HttpExtensions
    {
        private const char QueryStringDelimiter = ',';

        private const char ResourcePathDelimiter = '/';

        public static void AddQueryParams(this HttpRequest httpRequest, string name, string value)
        {
            httpRequest.QueryString = httpRequest.QueryString.Add(name, value);
        }

        public static string ExtendResourcePath(this string resource, string extension)
        {
            return String.Concat(resource, ResourcePathDelimiter, extension);
        }

        public static IEnumerable<string> ReadQueryParams(this HttpRequest httpRequest, string paramKey)
        {
            var aggregatedParamValues = new List<string>();

            var paramValues = httpRequest.Query[paramKey].ToArray();

            foreach (var value in paramValues)
            {
                aggregatedParamValues.AddRange(value.Split(QueryStringDelimiter));
            }

            return aggregatedParamValues;
        }
    }
}
