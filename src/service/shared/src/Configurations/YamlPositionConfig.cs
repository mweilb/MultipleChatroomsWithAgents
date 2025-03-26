using YamlDotNet.Serialization;

namespace  MultiAgents.Configurations
{

    public class YamlPositionConfig
    {
        [YamlMember(Alias = "x")]
        public int X { get; set; }

        [YamlMember(Alias = "y")]
        public int Y { get; set; }
    }
}
