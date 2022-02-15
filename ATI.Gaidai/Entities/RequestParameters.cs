using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;

namespace ATI.Gaidai.Entities
{
    public class RequestParameters
    {
        public JObject Parameters { get; set; }
        public IHeaderDictionary HeaderDictionary { get; set; }
        public IRequestCookieCollection Cookies { get; set; }
    }
}
