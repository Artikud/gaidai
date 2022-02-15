using System.Collections.Generic;
using JetBrains.Annotations;

namespace ATI.Gaidai.Entities
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public abstract class RequestDescriptor
    {
        public List<Parameter> TransitHeaders { get; set; } = new List<Parameter>();
        public Dictionary<string, string> AdditionalHeaders { get; set; } = new Dictionary<string, string>();
        public List<Parameter> TransitCookies { get; set; } = new List<Parameter>();
        public Dictionary<string, string> AdditionalCookies { get; set; } = new Dictionary<string, string>();
    }
}
