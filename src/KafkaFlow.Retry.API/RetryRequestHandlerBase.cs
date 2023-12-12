using System.IO;
using System.Text;
using System.Threading.Tasks;
using Dawn;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace KafkaFlow.Retry.API;

internal abstract class RetryRequestHandlerBase : IHttpRequestHandler
{

    private readonly string _path;
    private const string RetryResource = "retry";


    protected JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings()
    {
        DateTimeZoneHandling = DateTimeZoneHandling.Utc,
        TypeNameHandling = TypeNameHandling.None
    };

    protected abstract HttpMethod HttpMethod { get; }

    protected RetryRequestHandlerBase(string endpointPrefix, string resource)
    {
            Guard.Argument(resource, nameof(resource)).NotNull().NotEmpty();

            if (!string.IsNullOrEmpty(endpointPrefix))
            {
                _path = _path
                    .ExtendResourcePath(endpointPrefix);
            }

            _path = _path
                .ExtendResourcePath(RetryResource)
                .ExtendResourcePath(resource);
        }


    public virtual async Task<bool> HandleAsync(HttpRequest request, HttpResponse response)
    {
            if (!CanHandle(request))
            {
                return false;
            }

            await HandleRequestAsync(request, response).ConfigureAwait(false);

            return true;
        }

    protected bool CanHandle(HttpRequest httpRequest)
    {
            var resource = httpRequest.Path.ToUriComponent();

            if (!resource.Equals(_path))
            {
                return false;
            }

            var method = httpRequest.Method;

            if (!method.Equals(HttpMethod.ToString()))
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

            var requestDto = JsonConvert.DeserializeObject<T>(requestMessage, JsonSerializerSettings);

            return requestDto;
        }

    protected virtual async Task WriteResponseAsync<T>(HttpResponse response, T responseDto, int statusCode)
    {
            var body = JsonConvert.SerializeObject(responseDto, JsonSerializerSettings);

            response.ContentType = "application/json";
            response.StatusCode = statusCode;

            await response.WriteAsync(body, Encoding.UTF8).ConfigureAwait(false);
        }
}