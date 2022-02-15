using System.Collections.Generic;
using JetBrains.Annotations;

namespace ATI.Gaidai.Entities
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class Scenario
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public List<string> Methods { get; set; } = new List<string>();
        public bool IsDependent { get; set; }
        public bool IsForgettable { get; set; }
        public bool IsCommonSuccessful { get; set; }
        public bool IsRewritableResponse { get; set; }
        public List<string> ResponseMethods { get; set; } = new List<string>();
        public Dictionary<string, string> ResponseFieldTransformation { get; set; } = new Dictionary<string, string>();

    }
}
