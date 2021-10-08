namespace KafkaFlow.Retry.API
{
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;

    internal abstract class RetryRequestHandlerBase : IHttpRequestHandler
    {
        protected JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings()
        {
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            TypeNameHandling = TypeNameHandling.None
        };

        protected abstract HttpMethod HttpMethod { get; }

        protected virtual string ResourcePath => "/retry";

        public virtual async Task<bool> HandleAsync(HttpRequest request, HttpResponse response)
        {
            if (!this.CanHandle(request))
            {
                return false;
            }

            await this.HandleRequestAsync(request, response).ConfigureAwait(false);

            return true;
        }

        protected bool CanHandle(HttpRequest httpRequest)
        {
            var resource = httpRequest.Path.ToUriComponent();

            if (!resource.Equals(this.ResourcePath))
            {
                return false;
            }

            var method = httpRequest.Method;

            if (!method.Equals(this.HttpMethod.ToString()))
            {
                return false;
            }

            return true;
        }

        protected abstract Task HandleRequestAsync(HttpRequest request, HttpResponse response);

        protected virtual async Task<T> ReadRequestDtoAsync<T>(HttpRequest request)
        {
            string requestMessage;

            using (var reader = new StreamReader(request.Body, Encoding.UTF8))
            {
                requestMessage = await reader.ReadToEndAsync().ConfigureAwait(false);
            }

            var requestDto = JsonConvert.DeserializeObject<T>(requestMessage, this.jsonSerializerSettings);

            return requestDto;
        }

        protected virtual Task WriteResponseAsync<T>(HttpResponse response, T responseDto, int statusCode)
        {
            var body = JsonConvert.SerializeObject(responseDto, this.jsonSerializerSettings);

            response.ContentType = "application/json";
            response.StatusCode = statusCode;

            return response.WriteAsync(body, Encoding.UTF8);
        }
    }
}
