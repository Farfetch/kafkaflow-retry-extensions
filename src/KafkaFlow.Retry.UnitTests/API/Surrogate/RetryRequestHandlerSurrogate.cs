namespace KafkaFlow.Retry.UnitTests.API.Surrogate
{
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using System.Threading.Tasks;
    using global::KafkaFlow.Retry.API;
    using Microsoft.AspNetCore.Http;

    [ExcludeFromCodeCoverage]
    internal class RetryRequestHandlerSurrogate : RetryRequestHandlerBase
    {
        protected override HttpMethod HttpMethod => HttpMethod.GET;

        protected override async Task HandleRequestAsync(HttpRequest request, HttpResponse response)
        {
            var requestDto = await this.ReadRequestDtoAsync<DtoSurrogate>(request);

            var responseDto = requestDto;

            await this.WriteResponseAsync(response, responseDto, (int)HttpStatusCode.OK);
        }
    }
}