using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace KafkaFlow.Retry.API;

internal interface IHttpRequestHandler
{
    Task<bool> HandleAsync(HttpRequest httpRequest, HttpResponse response);
}