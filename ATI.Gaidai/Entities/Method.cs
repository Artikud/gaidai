using System.Collections.Generic;
using ATI.Gaidai.Enums;
using JetBrains.Annotations;

namespace ATI.Gaidai.Entities
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class Method : RequestDescriptor
    {
        public string Id { get; set; }
        public string ServiceId { get; set; }
        public string Url { get; set; }
        public HttpMethod HttpMethod { get; set; }
        public List<MethodParameter> Parameters { get; set; } = new List<MethodParameter>();
        public string ErrorReasonField { get; set; }
        public Dictionary<string,string> ResponseFieldTransformation { get; set; } = new Dictionary<string, string>();

    }
}
