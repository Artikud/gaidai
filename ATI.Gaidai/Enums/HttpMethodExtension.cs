using System;

namespace ATI.Gaidai.Enums
{
    public static class HttpMethodExtension
    {
        public static System.Net.Http.HttpMethod GetHttpRequestMethod(this HttpMethod method)
        {
            switch (method)
            {
                case HttpMethod.Get:
                    return System.Net.Http.HttpMethod.Get;
                case HttpMethod.Post:
                    return System.Net.Http.HttpMethod.Post;
                case HttpMethod.Delete:
                    return System.Net.Http.HttpMethod.Delete;
                case HttpMethod.Options:
                    return System.Net.Http.HttpMethod.Options;
                case HttpMethod.Put:
                    return System.Net.Http.HttpMethod.Put;
                case HttpMethod.Patch:
                    return System.Net.Http.HttpMethod.Patch;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}