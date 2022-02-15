using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using ATI.Gaidai.Entities;
using ATI.Services.Common.Behaviors;
using Microsoft.AspNetCore.Http;

namespace ATI.Gaidai.Helpers
{
    public class CookiesHelper
    {
        public OperationResult FillCookies(Service service, Method method, IRequestCookieCollection cookieCollection, HttpRequestMessage requestMessage)
        {
            var transitCookies = new HashSet<Parameter>(service.TransitCookies.Union(method.TransitCookies));
            var cookies = new StringBuilder();
            foreach (var transitCookie in transitCookies)
            {
                if (cookieCollection.TryGetValue(transitCookie.Name, out var cookieValue))
                {
                    cookies.Append($"{transitCookie.Name}={cookieValue};");
                }
                else
                {
                    if (transitCookie.Required)
                    {
                        return new OperationResult<HttpRequestMessage>(
                            ActionStatus.BadRequest,
                            $"Request does not contains required {transitCookie.Name} cookie");
                    }
                }
            }
            foreach (var additionalCookie in service.AdditionalCookies.Union(method.AdditionalCookies))
            {
                cookies.Append($"{additionalCookie.Key}={additionalCookie.Value};");
            }
            
            if (cookies.Length > 0)
            {
                requestMessage.Headers.Add("Cookie", cookies.ToString());
            }
            return OperationResult.Ok;
        }
    }
}
