using System;
using System.Collections.Generic;
using System.Linq;
using Dawn;

namespace KafkaFlow.Retry.API.Adapters.Common.Parsers;

internal class EnumParser<T> : IQueryParametersParser<T> where T : struct
{
    public IEnumerable<T> Parse(IEnumerable<string> parameters, IEnumerable<T> defaultValue)
    {
            Guard.Argument(parameters, nameof(parameters)).NotNull();
            Guard.Argument(defaultValue, nameof(defaultValue)).NotNull();

            var items = new List<T>();

            if (parameters.Any())
            {
                foreach (var param in parameters)
                {
                    if (Enum.TryParse<T>(param, out var item))
                    {
                        items.Add(item);
                    }
                }

                return items;
            }

            return defaultValue;
        }
}