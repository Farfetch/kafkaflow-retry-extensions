namespace KafkaFlow.Retry.API
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

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
            var handled = await this.httpRequestHandler
                .HandleAsync(httpContext.Request, httpContext.Response)
                .ConfigureAwait(false);

            if (!handled)
            {
                // Call the next delegate/middleware in the pipeline
                await this.next(httpContext).ConfigureAwait(false);
            }
        }
    }
}
