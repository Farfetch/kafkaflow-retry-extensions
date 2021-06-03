namespace KafkaFlow.Retry.API.Adapters.Common.Parsers
{
    using System.Collections.Generic;

    internal interface IQueryParametersParser<T> where T : struct
    {
        IEnumerable<T> Parse(IEnumerable<string> parameters, IEnumerable<T> defaultValue);
    }
}
