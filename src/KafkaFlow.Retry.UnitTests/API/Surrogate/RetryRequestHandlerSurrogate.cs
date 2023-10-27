namespace KafkaFlow.Retry.UnitTests.API.Surrogate
{
    using System.Net;
    using System.Threading.Tasks;
    using global::KafkaFlow.Retry.API;
    using Microsoft.AspNetCore.Http;

    internal class RetryRequestHandlerSurrogate : RetryRequestHandlerBase
    {

        public RetryRequestHandlerSurrogate(string endpointPrefix, string resource) : base(endpointPrefix, resource)
        {
        }

        protected override HttpMethod HttpMethod => HttpMethod.GET;

        protected override async Task HandleRequestAsync(HttpRequest request, HttpResponse response)
        {
            var requestDto = await this.ReadRequestDtoAsync<DtoSurrogate>(request);

            var responseDto = requestDto;

            await this.WriteResponseAsync(response, responseDto, (int)HttpStatusCode.OK);
        }
    }
}