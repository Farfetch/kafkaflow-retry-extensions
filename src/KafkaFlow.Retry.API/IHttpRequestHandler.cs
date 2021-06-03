namespace KafkaFlow.Retry.API
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

    internal interface IHttpRequestHandler
    {
        Task<bool> HandleAsync(HttpRequest httpRequest, HttpResponse response);
    }
}
