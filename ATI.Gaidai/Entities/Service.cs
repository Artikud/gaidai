using System;
using JetBrains.Annotations;

namespace ATI.Gaidai.Entities
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class Service : RequestDescriptor
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public Uri Path { get; set; }
    }
}
