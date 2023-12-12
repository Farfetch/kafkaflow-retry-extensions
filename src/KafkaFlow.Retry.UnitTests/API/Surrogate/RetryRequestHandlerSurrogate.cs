using System.Net;
using System.Threading.Tasks;
using KafkaFlow.Retry.API;
using Microsoft.AspNetCore.Http;

namespace KafkaFlow.Retry.UnitTests.API.Surrogate;

internal class RetryRequestHandlerSurrogate : RetryRequestHandlerBase
{

    public RetryRequestHandlerSurrogate(string endpointPrefix, string resource) : base(endpointPrefix, resource)
    {
    }

    protected override HttpMethod HttpMethod => HttpMethod.GET;

    protected override async Task HandleRequestAsync(HttpRequest request, HttpResponse response)
    {
        var requestDto = await ReadRequestDtoAsync<DtoSurrogate>(request);

        var responseDto = requestDto;

        await WriteResponseAsync(response, responseDto, (int)HttpStatusCode.OK);
    }
}