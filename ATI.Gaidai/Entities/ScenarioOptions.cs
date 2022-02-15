using System.Collections.Generic;
using JetBrains.Annotations;

namespace ATI.Gaidai.Entities
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public class ScenarioOptions
    {
        public HashSet<Service> Services { get; set; }
        public HashSet<Method> Methods { get; set; }
        public HashSet<Scenario> Scenarios { get; set; }
    }
}
