namespace KafkaFlow.Retry.API
{
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;

    internal abstract class RetryRequestHandlerBase : IHttpRequestHandler
    {

        private readonly string path;
        private const string RetryResource = "retry";
        private const string Delimiter = "/";


        protected JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings()
        {
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            TypeNameHandling = TypeNameHandling.None
        };

        protected abstract HttpMethod HttpMethod { get; }

        protected RetryRequestHandlerBase(string endpointPrefix, string resource)
        {

            if (!string.IsNullOrEmpty(endpointPrefix))
            {
                this.path = endpointPrefix.ExtendResourcePath(RetryResource);
                if (!string.IsNullOrEmpty(resource))
                {
                    this.path = this.path.ExtendResourcePath(resource);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(resource))
                {
                    this.path = RetryResource.ExtendResourcePath(resource);
                }
                else
                {
                    this.path = RetryResource;
                }
            }

            this.path = Delimiter + this.path;
        }




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

            if (!resource.Equals(this.path))
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

        protected virtual async Task WriteResponseAsync<T>(HttpResponse response, T responseDto, int statusCode)
        {
            var body = JsonConvert.SerializeObject(responseDto, this.jsonSerializerSettings);

            response.ContentType = "application/json";
            response.StatusCode = statusCode;

            await response.WriteAsync(body, Encoding.UTF8).ConfigureAwait(false);
        }
    }
}
