using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using ATI.Gaidai.Entities;
using ATI.Services.Common.Behaviors;
using Microsoft.Extensions.Primitives;

namespace ATI.Gaidai.Helpers
{
    public class HeadersHelper
    {
        public OperationResult FillHeaders(Service service, Method method, IDictionary<string, StringValues> headerDictionary, HttpRequestMessage requestMessage)
        {
            var transitHeaders = new HashSet<Parameter>(service.TransitHeaders.Union(method.TransitHeaders));

            foreach (var transitHeader in transitHeaders)
            {
                if (headerDictionary.TryGetValue(transitHeader.Name, out var headerValue))
                {
                    requestMessage.Headers.Add(transitHeader.Name, headerValue.ToArray());
                }
                else
                {
                    if (transitHeader.Required)
                    {
                        return new OperationResult<HttpRequestMessage>(ActionStatus.BadRequest, $"Request does not contains required {transitHeader.Name} header");
                    }
                }
            }
            foreach (var additionalHeader in service.AdditionalHeaders.Union(method.AdditionalHeaders))
            {
                requestMessage.Headers.Add(additionalHeader.Key, additionalHeader.Value);
            }

            return OperationResult.Ok;
        }
    }
}
