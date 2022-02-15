using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ATI.Gaidai.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum HttpMethod
    {
        Get,
        Post,
        Delete,
        Put,
        Options,
        Patch
    }
}
