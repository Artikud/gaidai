using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ATI.Gaidai.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ParameterDestination
    {
        Url,
        Body,
        Header
    }
}
