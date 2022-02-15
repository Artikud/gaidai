using System;
using System.Net.Http;
using ATI.Gaidai.Entities;
using ATI.Gaidai.Enums;
using ATI.Services.Common.Behaviors;

namespace ATI.Gaidai.Helpers
{
    public class HttpRequestHelper
    {
        private readonly ParametersHelper _parametersHelper;
        private readonly HeadersHelper _headersHelper;
        private readonly CookiesHelper _cookiesHelper;

        public HttpRequestHelper(ParametersHelper parametersHelper, HeadersHelper headersHelper, CookiesHelper cookiesHelper)
        {
            _parametersHelper = parametersHelper;
            _headersHelper = headersHelper;
            _cookiesHelper = cookiesHelper;
        }

        public OperationResult<HttpRequestMessage> GetRequestMessage(Service service, Method method, RequestParameters requestParameters)
        {
            var requestMessage = new HttpRequestMessage { Method = method.HttpMethod.GetHttpRequestMethod() };

            var methodUriPart = method.Url;

            var fillParamsResult = _parametersHelper.FillParameters(requestMessage, method, requestParameters.Parameters, ref methodUriPart);

            if (!fillParamsResult.Success)
            {
                return new OperationResult<HttpRequestMessage>(fillParamsResult);
            }

            requestMessage.RequestUri = new Uri(service.Path, methodUriPart);

            var fillHeadersResult = _headersHelper.FillHeaders(service, method, requestParameters.HeaderDictionary, requestMessage);

            if (!fillHeadersResult.Success)
            {
                return new OperationResult<HttpRequestMessage>(fillHeadersResult);
            }

            var fillCookiesResult = _cookiesHelper.FillCookies(service, method, requestParameters.Cookies, requestMessage);

            if (!fillCookiesResult.Success)
            {
                return new OperationResult<HttpRequestMessage>(fillCookiesResult);
            }

            return new OperationResult<HttpRequestMessage>(requestMessage);
        }
    }
}
