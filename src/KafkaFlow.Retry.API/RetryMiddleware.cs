using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace KafkaFlow.Retry.API;

internal class RetryMiddleware
{
    private readonly IHttpRequestHandler httpRequestHandler;
    private readonly RequestDelegate next;

    public RetryMiddleware(RequestDelegate next, IHttpRequestHandler httpRequestHandler)
    {
        this.next = next;
        this.httpRequestHandler = httpRequestHandler;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        var handled = await httpRequestHandler
            .HandleAsync(httpContext.Request, httpContext.Response)
            .ConfigureAwait(false);

        if (!handled)
        {
            // Call the next delegate/middleware in the pipeline
            await next(httpContext).ConfigureAwait(false);
        }
    }
}