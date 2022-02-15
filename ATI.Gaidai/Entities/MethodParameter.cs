using ATI.Gaidai.Enums;

namespace ATI.Gaidai.Entities
{
    public class MethodParameter : Parameter
    {
        public ParameterDestination Destination { get; set; }
        public int? UrlPosition { get; set; }
        public string SourceMethod { get; set; }
        public bool UnnamedBody { get; set; }
    }
}
