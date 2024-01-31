using System.Collections.Generic;

namespace KafkaFlow.Retry.API.Adapters.Common.Parsers;

internal interface IQueryParametersParser<T> where T : struct
{
    IEnumerable<T> Parse(IEnumerable<string> parameters, IEnumerable<T> defaultValue);
}