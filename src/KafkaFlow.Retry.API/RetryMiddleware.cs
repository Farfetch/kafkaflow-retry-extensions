using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace KafkaFlow.Retry.API;

internal class RetryMiddleware
{
    private readonly IHttpRequestHandler _httpRequestHandler;
    private readonly RequestDelegate _next;

    public RetryMiddleware(RequestDelegate next, IHttpRequestHandler httpRequestHandler)
    {
        _next = next;
        _httpRequestHandler = httpRequestHandler;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        var handled = await _httpRequestHandler
            .HandleAsync(httpContext.Request, httpContext.Response)
            .ConfigureAwait(false);

        if (!handled)
        {
            // Call the next delegate/middleware in the pipeline
            await _next(httpContext).ConfigureAwait(false);
        }
    }
}